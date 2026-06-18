#!/bin/bash
set -e

echo "⏳ Laukiama MySQL paleidimo..."
until nc -z -v -w30 mysql 3306
do
  echo "⏳ MySQL dar nepasileido..."
  sleep 2
done

echo "🚀 MySQL pasiekiamas! Paleidžiamos migracijos..."
dotnet ef database update --project ../TaskSystem.Data --startup-project .

echo "✅ Migracijos pritaikytos!"
echo "🚀 Paleidžiamas API..."

dotnet TaskSystem.API.dll
