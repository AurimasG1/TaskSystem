# 🚀 TaskSystem – Užduočių valdymo REST API

## 🟦 1. Kaip paleisti projektą lokaliai

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

## 🏗 Projekto architektūra

Projektas sukurtas pagal **švarią sluoksninę architektūrą**, kuri užtikrina aiškų atsakomybės paskirstymą, lengvą testavimą ir gerą kodo palaikymą.

### 🔹 1. API sluoksnis (`TaskSystem.API`)

Atsakingas už:

- HTTP užklausų priėmimą
- REST endpoint’ų aprašymą
- Modelių validaciją
- Autentifikaciją ir autorizaciją (JWT)
- Atsakymų formavimą

Naudoja:

- Controllerius (`AuthController`, `UserController`, `AdminController`)
- `[ApiController]`, `[Authorize]`, `[AllowAnonymous]`

---

### 🔹 2. Servisų sluoksnis (`TaskSystem.Services`)

Atsakingas už:

- verslo logiką
- validaciją
- darbo su repository koordinavimą
- papildomas taisykles (pvz., naudotojas gali valdyti tik savo užduotis)

Pavyzdžiai:

- `UserService`
- `UzduotisService`
- `JwtService`

---

### 🔹 3. Repository sluoksnis (`TaskSystem.Repositories`)

Atsakingas už:

- duomenų prieigą
- CRUD operacijas
- EF Core užklausas

Pavyzdžiai:

- `UserRepository`
- `UzduotisRepository`

Repository naudoja:

- `AppDbContext`
- EF Core 9
- Pomelo MySQL provider

---

### 🔹 4. Duomenų sluoksnis (`TaskSystem.Data`)

Atsakingas už:

- duomenų bazės kontekstą (`AppDbContext`)
- DbSet’ų aprašymą
- MySQL konfigūraciją

---

### 🔹 5. Bendri modeliai (`TaskSystem.Common`)

Atsakingi už:

- DTO modelius
- Entity modelius
- Enum’us
- Validacijos atributus

---

### 📌 Architektūros principai

- **Dependency Injection** naudojamas visur (services, repositories)
- **Controlleriai neturi verslo logikos**
- **Service sluoksnis neturi tiesioginės prieigos prie DB**
- **Repository neturi verslo logikos**
- **DTO atskirti nuo Entity**
- **JWT autentifikacija ir rolėmis paremta autorizacija**
- **User Secrets / Environment Variables** naudojami konfigūracijai

Tokiu būdu projektas yra:

- lengvai testuojamas
- plečiamas
- saugus
- aiškiai struktūruotas

## 🔐 Autentifikacija ir autorizacija

Projektas naudoja **JWT (JSON Web Token)** autentifikaciją ir rolėmis paremtą autorizaciją.

---

# 1. Autentifikacija (JWT)

Autentifikacija vyksta per du endpoint’us:

| Metodas | Endpoint             | Aprašymas                          |
| ------- | -------------------- | ---------------------------------- |
| POST    | `/api/auth/register` | Registracija                       |
| POST    | `/api/auth/login`    | Prisijungimas (grąžina JWT tokeną) |

Prisijungus, API grąžina JWT tokeną, kurį naudotojas turi pateikti `Authorization` header’yje:
