# Ozon Route 256 — Kafka Homework

## Домашнее задание
Наш маленький стартап Кислород.Ру перерос в серьезный бизнес, и нам нужно научиться в учёт проданного товара, а также (опционально) в аналитику продаваемого товара

В проде у нас есть сервис заказов, который при создании заказа отправляет в топик Кафки событие об этом (в рамках ДЗ его роль выполняет консольное приложение, которое нещадно генерирует события в этот топик). 

Вот формат этого события:
```json
{
    "moment": timestamp,
    "order_id": long,
    "user_id": long,
    "warehouse_id": long,
    "positions": {
        "item_id": long,
        "quantity": int,
        "price": {
            "currency: "RUR|KZT",
            "units": long,
            "nanos": long
        }
    },
    "status": "created|cancelled|delivered"
}
```

Наши задача — написать сервис, который будет читать этот топик и считать витрину данных:
* Учет товаров — по каждому item_id: сколько зарезервировано (created), сколько продано (переход created → delivered), сколько отменено (created → cancelled)
* 💎 Учёт денежных средств к уплате каждому продавцу — item_id — составной



## Docker cheat sheet
* Если у вас всё ещё нет docker — его нужно поставить.
* Запустить docker контейнеры (БД): `docker compose up -d`
* Остановить docker контейнеры (БД): `docker compose down`
* Остановить и почистить docker от данных: `docker compose down -v`
* Docker поломался: `docker system prune`


## Kafka cheat sheet
* Заводите Kafka через docker. Пример docker-compose-файла с Kafka находится в репозитории воркшопа этой недели: https://gitlab.ozon.dev/cs/classroom-6/Week-6/workshop/-/blob/master/docker-compose.yml
* Откройте ваш hosts-файл (для windows это `c:\windows\system32\drivers\etc\hosts`) Добавьте туда строчку `127.0.0.1 kafka`.
* Offset Explorer (ранее называлось Kafka Tool): https://www.kafkatool.com/ — позволяет читать и писать в Apache Kafka через простой UI. Да, писать и читать protobuf почти нереально. Но это отдельная боль в нашем мире.
* Как настроить Offset Explorer? 
* * Clusters → Add New Connection
* * Cluster Name → Любое имя
* * Вкладка Advanced → Bootstrap Servers → написать `kafka:9092`
* * Test, Add
* Серьезные девчата и пацаны качают Kafka (https://kafka.apache.org/downloads) и используют sh/bat-файлы оттуда, чтобы работать с локальной/докер/стейдж/прод Kafka. Но мы тут все несерьезные, и используем такое только в случае крайней необходимости.