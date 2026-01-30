![build](https://github.com/planara/planara-accounts/actions/workflows/build.yml/badge.svg)

## Planara.Accounts

Сервис управления пользовательскими профилями.

Отвечает за хранение и обновление публичных и персональных данных пользователя
(профиль, отображаемое имя, никнейм, био, аватар и т.д.).
Интегрируется с сервисом аутентификации через Kafka события.

Реализован как ASP.NET Core + GraphQL сервис с JWT-аутентификацией.

## Features

- Хранение профиля пользователя
- Автоматическое создание профиля при регистрации пользователя (Kafka)
- Частичное обновление профиля
- JWT авторизация (`[Authorize]`)
- Валидация входных данных (FluentValidation)
- GraphQL API (HotChocolate)
- Kafka consumer (at-least-once delivery)

## GraphQL API

### Queries

- `getProfile: Profile`  
  Возвращает профиль текущего пользователя  
  _(требует авторизации)_

### Mutations

- `updateProfile(request: UpdateProfileRequest): Profile`  
  Обновляет профиль текущего пользователя  
  Поддерживает частичное обновление (обновляются только переданные поля)  
  _(требует авторизации)_