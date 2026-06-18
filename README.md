# 🚀 TaskSystem – Užduočių valdymo REST API

# 🧰 Naudojamos technologijos ir versijos

Projektas sukurtas naudojant:

- **.NET SDK 10.0**  
  (naudojamas kaip pagrindinis kompiliavimo ir paleidimo SDK)

- **ASP.NET Core 9.0**  
  (naujausia stabili ASP.NET Core versija, suderinama su .NET 10)

- **Entity Framework Core 9.0**  
  (naujausia stabili EF Core versija)

- **Pomelo.EntityFrameworkCore.MySql 9.0.0**  
  (suderinta su EF Core 9)

- **Swashbuckle.AspNetCore 6.5.0**  
  (Swagger generavimui)

- **System.IdentityModel.Tokens.Jwt 8.19.1**  
  (JWT generavimui ir validavimui)

## Kodėl SDK 10, o paketai 9?

.NET 10 SDK yra naujausias, tačiau ASP.NET Core ir EF Core 10 versijos dar nėra išleistos.  
Todėl naudojamos **naujausios stabilios 9.x versijos**, kurios yra pilnai suderinamos su .NET 10.

Tai yra standartinė praktika .NET ekosistemoje.

---

# 🟦 1. Kaip paleisti projektą lokaliai

## 1.1. Klonuoti repozitoriją

```bash
git clone https://github.com/<tavo-repo>/TaskSystem.git
cd TaskSystem/TaskSystem.API
```

## 1.2. Paleisti MySQL duomenų bazę (Docker)

```
docker run --name mysql8 \
 -e MYSQL_ROOT_PASSWORD=root \
 -e MYSQL_DATABASE=tasksystem \
 -p 3306:3306 \
 -d mysql:8
```

arba

## 🐳 Projekto paleidimas naudojant Docker Compose

Projektą galima paleisti vienu mygtuku naudojant `docker-compose.yml`, kuris startuoja:

- MySQL 8 duomenų bazę
- TaskSystem.API projektą
- automatiškai perduoda konfigūracijos kintamuosius (JWT Key, ConnectionString)

### Paleidimas

```bash
docker compose up -d
```

## 1.3. Nustatyti User Secrets (būtina!)

```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=tasksystem;user=root;password=root"
dotnet user-secrets set "Jwt:Key" "ĮRAŠYK_SAVO_SLAPTA_RAKTĄ"
dotnet user-secrets set "Jwt:Issuer" "TaskSystemAPI"
```

Patikrinti

```
dotnet user-secrets list
```

## 1.4. Paleisti API

```
cd TaskSystem/TaskSystem.API/
dotnet run
```

API bus pasiekiamas

```
https://localhost:5263/swagger
```

# 2. REST API endpoint'ai

## 2.1. Autentifikacija (be JWT)

| Metodas | Endpoint             | Aprašymas                          |
| ------- | -------------------- | ---------------------------------- |
| POST    | `/api/auth/register` | Registracija                       |
| POST    | `/api/auth/login`    | Prisijungimas (grąžina JWT tokeną) |

## 2.2. Vartoto (User) endpoint'ai

Reikalauja JWT tokeno

| Metodas | Endpoint                  | Aprašymas                           |
| ------- | ------------------------- | ----------------------------------- |
| POST    | `/api/user/uzduotys`      | Sukuria naują užduotį               |
| GET     | `/api/user/uzduotys/my`   | Grąžina visas naudotojo užduotis    |
| GET     | `/api/user/uzduotys/last` | Grąžina paskutinę naudotojo užduotį |
| PUT     | `/api/user/uzduotys/{id}` | Atnaujina naudotojo užduotį         |
| DELETE  | `/api/user/uzduotys/{id}` | Ištrina naudotojo užduotį           |

## 2.3. Administratoriaus (Admin) endpoint’ai

Reikalauja JWT tokeno ir rolės admin

| Metodas | Endpoint                                  | Aprašymas                                      |
| ------- | ----------------------------------------- | ---------------------------------------------- |
| GET     | `/api/admin/uzduotys`                     | Grąžina visas užduotis                         |
| GET     | `/api/admin/uzduotys/by-user/{userId}`    | Grąžina konkretaus naudotojo užduotis          |
| GET     | `/api/admin/uzduotys/last/{userId}`       | Grąžina konkretaus naudotojo paskutinę užduotį |
| PUT     | `/api/admin/uzduotys/reset-last/{userId}` | Atstato paskutinės užduoties įrašą             |

# 3. Kaip paleisti vienetinius testus

Testai yra projekte TaskSystem.Tests

```
dotnet test

```

Testuose naudojama:

xUnit – testų framework

Moq – repository imitavimui

EF Core InMemory – izoliuotam testavimui be tikros DB

Testuojama:

Užduoties validacija (Title privalomas)

Paskutinės užduoties gavimas

UpdatedAt atnaujinimas

Repository metodų kvietimas (ResetLastUzduotisAsync)

# 4. Saugumo taisyklės (Guard Rails)

Visi endpoint’ai, išskyrus /register ir /login, reikalauja JWT tokeno

User gali valdyti tik savo užduotis

Admin turi prieigą prie visų užduočių

API raktai ir DB slaptažodžiai nėra saugomi GitHub’e

Jautri informacija perduodama per:

User Secrets (lokaliai)

Environment Variables (production / Docker)

appsettings.json saugo tik struktūrą, be slaptažodžių

.gitignore blokuoja:

secrets.json

.env

appsettings.Development.json
