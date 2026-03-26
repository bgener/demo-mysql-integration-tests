#!/bin/bash
# create-baseline.sh
# Copies migration files and builds the baseline Docker image

MIGRATIONS_SRC="../src/OrderSystem/Migrations"
MIGRATIONS_DST="./Migrations"

# Copy migrations into baselines folder for Docker build context
rm -rf "$MIGRATIONS_DST"
mkdir -p "$MIGRATIONS_DST"
cp "$MIGRATIONS_SRC"/*.sql "$MIGRATIONS_DST/"

echo "Migrations copied to $MIGRATIONS_DST"

# Build the baseline image
docker build -t mysql-test-baseline:latest .

echo "Baseline image built: mysql-test-baseline:latest"
