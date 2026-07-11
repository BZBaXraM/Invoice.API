# Деплой на AWS (EC2 + Docker Compose + GitHub Actions)

Пайплайн: пуш в `main` → [ci.yml](.github/workflows/ci.yml) собирает API и клиент для проверки, [deploy.yml](.github/workflows/deploy.yml) собирает Docker-образы, пушит их в GHCR (`ghcr.io/bzbaxram/invoice-api`, `ghcr.io/bzbaxram/invoice-client`) и по SSH перезапускает стек на EC2 через [docker-compose.prod.yml](docker-compose.prod.yml).

Схема на сервере: Caddy (порты 80/443, автоматический Let's Encrypt для домена) → `/api/*` и `/hubs/*` идут в API-контейнер (HTTP :8080), всё остальное — в nginx-контейнер клиента. Клиент и API — **один origin**, Postgres наружу не торчит. Swagger снаружи недоступен (Caddy его не маршрутизирует).

## Разовая настройка

### 1. EC2

- Инстанс: Ubuntu 24.04 LTS, `t3.small` (2 ГБ RAM — хватает на Postgres + API + nginx + Caddy), диск 20 ГБ gp3.
- Security group: входящие `22` (SSH, лучше только со своего IP), `80`, `443`.
- Прикрепите **Elastic IP**, чтобы адрес не менялся при перезапуске.

На сервере:

```bash
# Docker + compose plugin
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker ubuntu   # перелогиньтесь после этого

# Каталог деплоя (владелец — пользователь, под которым ходит CI)
sudo mkdir -p /opt/invoice && sudo chown ubuntu:ubuntu /opt/invoice
```

### 2. DNS

A-запись вашего домена → Elastic IP. Caddy сам получит сертификат при первом запуске (домен уже должен резолвиться в IP сервера, иначе выдача упадёт).

### 3. `/opt/invoice/.env` на сервере

Создаётся вручную один раз (в git не попадает):

```bash
DOMAIN=invoice.example.com

POSTGRES_USER=postgres
POSTGRES_PASSWORD=<сильный пароль>
POSTGRES_DB=InvoiceDb

# минимум 64 символа (HmacSha512)
JWT_SECRET=<64+ случайных символов>

EMAIL_FROM=...
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USERNAME=...
EMAIL_PASSWORD=...

GROQ_API_KEY=...
```

Помните про gotcha из CLAUDE.md: сменить `POSTGRES_PASSWORD` на живом volume нельзя — Postgres запомнил старый при инициализации (`docker compose down -v` стирает данные).

### 4. Secrets и Variables в GitHub

`Settings → Secrets and variables → Actions`:

| Тип | Имя | Значение |
|---|---|---|
| Secret | `EC2_HOST` | Elastic IP или домен сервера |
| Secret | `EC2_USER` | `ubuntu` |
| Secret | `EC2_SSH_KEY` | приватный SSH-ключ (PEM целиком) для этого пользователя |
| Variable | `DOMAIN` | `invoice.example.com` — запекается в клиентский билд как `https://$DOMAIN/api` |

`GITHUB_TOKEN` для GHCR отдельно настраивать не нужно — workflow пушит и логинит сервер сам.

### 5. Первый деплой

Пуш в `main` (или `Actions → Deploy → Run workflow`). Проверить на сервере: `docker compose -f /opt/invoice/docker-compose.prod.yml ps` и `logs api`.

## Что осталось за кадром (осознанно)

- **Бэкапы БД** — Postgres живёт в Docker-volume на инстансе. Минимум: cron с `docker compose exec postgres pg_dump ... | gzip > backup.sql.gz` и выгрузкой в S3.
- **Миграции** применяются автоматически при старте API (`InitialiseDatabaseAsync()` в `Program.cs`) — отдельного шага в CI нет.
- Смена `vars.DOMAIN` требует пересборки клиентского образа (URL запекается на этапе билда) — просто перезапустите Deploy-workflow.
