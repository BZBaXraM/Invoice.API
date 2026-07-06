export type LanguageCode = 'ru' | 'eng' | 'az';

export interface LanguageOption {
  code: LanguageCode;
  label: string;
}

export const SUPPORTED_LANGUAGES: LanguageOption[] = [
  { code: 'ru', label: 'Русский' },
  { code: 'eng', label: 'English' },
  { code: 'az', label: 'Azərbaycan' },
];

export const DEFAULT_LANGUAGE: LanguageCode = 'ru';

export const LOCALE_MAP: Record<LanguageCode, string> = {
  ru: 'ru-RU',
  eng: 'en-GB',
  az: 'az-Latn-AZ',
};

export interface TranslationsResponse {
  language: string;
  translations: Record<string, string>;
}
