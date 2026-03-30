# Stage 1: Initialize MySQL, run migrations, and create a dump
FROM mysql:8.0 AS initializer

ENV MYSQL_ROOT_PASSWORD=root

COPY src/OrderSystem/Migrations/ /migrations/

RUN mkdir -p /var/lib/mysql-init && chown mysql:mysql /var/lib/mysql-init

RUN set -eux; \
    mysqld --user=mysql --initialize-insecure --datadir=/var/lib/mysql-init; \
    mysqld --user=mysql --datadir=/var/lib/mysql-init --skip-networking & \
    pid=$!; \
    for i in $(seq 1 120); do \
        mysqladmin --socket=/var/run/mysqld/mysqld.sock ping 2>/dev/null && break; \
        sleep 1; \
    done; \
    mysql --socket=/var/run/mysqld/mysqld.sock -uroot \
        -e "CREATE DATABASE IF NOT EXISTS baseline;"; \
    for f in $(ls /migrations/*.sql | sort); do \
        echo "Running migration: $f"; \
        mysql --socket=/var/run/mysqld/mysqld.sock -uroot baseline < "$f"; \
    done; \
    mysqldump --socket=/var/run/mysqld/mysqld.sock -uroot \
        --routines --triggers baseline > /baseline.sql; \
    mysqladmin --socket=/var/run/mysqld/mysqld.sock -uroot shutdown; \
    wait $pid; \
    cp -r /var/lib/mysql-init /var/lib/mysql-data

# Stage 2: Final image with pre-initialized data directory and dump file
FROM mysql:8.0

COPY --from=initializer /var/lib/mysql-data /var/lib/mysql
COPY --from=initializer /baseline.sql /baseline.sql

ENV MYSQL_ROOT_PASSWORD=root
