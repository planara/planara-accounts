![build](https://github.com/planara/planara-accounts/actions/workflows/build.yml/badge.svg)
![deploy](https://github.com/planara/planara-accounts/actions/workflows/deploy.yml/badge.svg?branch=main)
![publish-k3s](https://github.com/planara/planara-accounts/actions/workflows/publish-k3s.yml/badge.svg?branch=main)
![version](https://img.shields.io/github/v/tag/planara/planara-accounts?sort=semver)
[![Codecov](https://codecov.io/gh/planara/planara-accounts/branch/main/graph/badge.svg)](https://codecov.io/gh/planara/planara-accounts)

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