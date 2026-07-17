# Деплой на AWS с нуля (EC2 + Docker Compose + GitHub Actions)

Пайплайн: пуш в `main` → [ci.yml](.github/workflows/ci.yml) собирает API и клиент для проверки, [deploy.yml](.github/workflows/deploy.yml) собирает Docker-образы, пушит их в GHCR (`ghcr.io/bzbaxram/invoice-api`, `ghcr.io/bzbaxram/invoice-client`) и по SSH перезапускает стек на EC2 через [docker-compose.prod.yml](docker-compose.prod.yml).

Схема на сервере: Caddy (порты 80/443, автоматический Let's Encrypt для домена) → `/api/*` и `/hubs/*` идут в API-контейнер (HTTP :8080), всё остальное — в nginx-контейнер клиента. Клиент и API — **один origin**, Postgres наружу не торчит. Swagger снаружи недоступен (Caddy его не маршрутизирует).

Ниже — полная настройка с чистого листа. Порядок важен.

## 1. Создать EC2-инстанс

Консоль AWS → EC2 → **Launch instance**:

| Поле | Значение |
|---|---|
| Name | `invoice-prod` |
| AMI | **Ubuntu Server 24.04 LTS** (не Amazon Linux — все команды ниже под Ubuntu, пользователь `ubuntu`) |
| Instance type | **t3.small** (2 ГБ RAM — стеку комфортно; t3.micro выживает только со swap) |
| Key pair | **Create new key pair** → имя `invoice-key`, тип ED25519 или RSA, формат `.pem`. Файл скачается один раз — положите его в `~/.ssh/invoice-key.pem` и сделайте `chmod 400 ~/.ssh/invoice-key.pem` |
| Network settings | Create security group, галочки: **Allow SSH traffic from → Anywhere** (0.0.0.0/0 — обязательно, GitHub Actions ходит с плавающих IP; вход всё равно только по ключу), **Allow HTTPS traffic from the internet**, **Allow HTTP traffic from the internet** |
| Storage | 20 GiB gp3 |

→ **Launch instance**.

## 2. Elastic IP (постоянный адрес)

Без него публичный IP меняется при каждом stop/start.

1. EC2 → Network & Security → **Elastic IPs** → **Allocate Elastic IP address** → Allocate (настройки по умолчанию).
2. Галочка на появившемся адресе → **Actions → Associate Elastic IP address** (не путать с оранжевой Allocate — она создаёт ещё один адрес).
3. В поле Instance выбрать `invoice-prod` — **сверяйте по instance-id**, если инстансов несколько → **Associate**.

Полученный адрес ниже называется `ELASTIC_IP`. Проверка доступа с вашего Mac:

```bash
ssh -i ~/.ssh/invoice-key.pem ubuntu@ELASTIC_IP
```

Elastic IP бесплатен, пока привязан к работающему инстансу; за «висящий» без инстанса — почасовая плата.

## 3. DNS

У регистратора домена: A-запись `invoice.bahram.site` → `ELASTIC_IP`. Caddy получит сертификат при первом запуске, но только когда домен уже резолвится в сервер (до этого будет ретраить — не страшно, просто сайт не откроется).

Проверка: `dig +short invoice.bahram.site` → должен вернуть `ELASTIC_IP`.

## 4. Настройка сервера

Зайдя по SSH (`ssh -i ~/.ssh/invoice-key.pem ubuntu@ELASTIC_IP`):

```bash
# Docker + compose plugin
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker ubuntu   # перелогиньтесь после этого (exit + ssh снова)

# Каталог деплоя (владелец — пользователь, под которым ходит CI)
sudo mkdir -p /opt/invoice && sudo chown ubuntu:ubuntu /opt/invoice

# Только если взяли t3.micro (1 ГБ RAM) — swap 2 ГБ; на t3.small не нужно
# sudo fallocate -l 2G /swapfile && sudo chmod 600 /swapfile
# sudo mkswap /swapfile && sudo swapon /swapfile
# echo "/swapfile none swap sw 0 0" | sudo tee -a /etc/fstab
```

## 5. `/opt/invoice/.env` на сервере

Создаётся вручную один раз, в git не попадает: `nano /opt/invoice/.env`, потом `chmod 600 /opt/invoice/.env`.

```bash
DOMAIN=invoice.bahram.site

POSTGRES_USER=postgres
POSTGRES_PASSWORD=<сильный пароль: openssl rand -hex 16>
POSTGRES_DB=InvoiceDb

# минимум 64 символа (HmacSha512): openssl rand -hex 48
JWT_SECRET=<64+ случайных символов>

EMAIL_FROM=invoice.aspnet@gmail.com
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USERNAME=invoice.aspnet@gmail.com
EMAIL_PASSWORD=<gmail app password — тот же, что в локальном .env репозитория>

GROQ_API_KEY=<тот же, что в локальном .env репозитория>
GROQ_MODEL=openai/gpt-oss-120b
```

Если Gmail начнёт отклонять отправку с EC2 ошибкой `534 WebLoginRequired` (Google периодически блокирует SMTP-логин с дата-центровых IP даже с app-паролем — такое уже случалось), запасной вариант — Brevo: `EMAIL_SMTP_SERVER=smtp-relay.brevo.com`, `EMAIL_PORT=587`, логин/ключ из аккаунта Brevo, `EMAIL_FROM` — подтверждённый там отправитель. В настройках Brevo функция Authorised IPs должна быть выключена или содержать `ELASTIC_IP`, иначе будет `525 Unauthorized IP address`. После правки `.env`: `docker compose -f /opt/invoice/docker-compose.prod.yml up -d api`.

Gotcha из CLAUDE.md: сменить `POSTGRES_PASSWORD` на живом volume нельзя — Postgres запомнил старый при инициализации (`docker compose down -v` стирает данные вместе с volume).

## 6. Secrets и Variables в GitHub

Репозиторий → `Settings → Secrets and variables → Actions`. Каждый секрет — **отдельной записью**: в Name только имя, в Secret только значение (не `ИМЯ=значение` и не всё скопом).

Вкладка **Secrets** → New repository secret, три раза:

| Имя | Значение |
|---|---|
| `EC2_HOST` | `ELASTIC_IP` |
| `EC2_USER` | `ubuntu` |
| `EC2_SSH_KEY` | вывод `cat ~/.ssh/invoice-key.pem` **целиком**, включая строки `-----BEGIN ... PRIVATE KEY-----` и `-----END ... PRIVATE KEY-----`, с переносами строк |

Вкладка **Variables** (соседняя, не Secrets!) → New repository variable:

| Имя | Значение |
|---|---|
| `DOMAIN` | `invoice.bahram.site` — запекается в клиентский билд как `https://$DOMAIN/api` |

`GITHUB_TOKEN` для GHCR отдельно настраивать не нужно — workflow пушит образы и логинит сервер сам.

## 7. Первый деплой

Любой пуш в `main`, либо вручную: GitHub → **Actions → Deploy → Run workflow**. Если предыдущий запуск падал — **Re-run all jobs** (именно all: если `DOMAIN` появилась после сборки, клиентский образ надо пересобрать).

Проверка после зелёного workflow:

```bash
# на сервере: 4 контейнера (postgres, api, client, caddy) в статусе Up
docker compose -f /opt/invoice/docker-compose.prod.yml ps
docker compose -f /opt/invoice/docker-compose.prod.yml logs api    # миграции применились?
docker compose -f /opt/invoice/docker-compose.prod.yml logs caddy  # сертификат выпустился?

# с любой машины
curl -I https://invoice.bahram.site
```

Типовые причины падения деплоя: шаг «Copy compose file...» — неверный `EC2_HOST`/`EC2_USER` или битый `EC2_SSH_KEY` (вставлен без BEGIN/END); сертификат не выпускается — DNS ещё не указывает на сервер или закрыт порт 80.

## Что осталось за кадром (осознанно)

- **Бэкапы БД** — на сервере настроен cron (03:00 UTC): `/opt/invoice/backup.sh` делает `pg_dump | gzip` в `/opt/invoice/backups/` с ротацией 14 дней. Бэкапы лежат на том же диске — выгрузка в S3 ещё не сделана (нужен bucket + IAM-роль).
- **Миграции** применяются автоматически при старте API (`InitialiseDatabaseAsync()` в `Program.cs`) — отдельного шага в CI нет.
- Смена `vars.DOMAIN` требует пересборки клиентского образа (URL запекается на этапе билда) — просто перезапустите Deploy-workflow.
