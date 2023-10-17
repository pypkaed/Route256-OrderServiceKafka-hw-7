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

### Домашнее задание №1 — сервис
Написать сервис, который будет читать этот топик и считать витрину данных:
* Учет товаров — по каждому item_id: сколько зарезервировано (created), сколько продано (переход created → delivered), сколько отменено (created → cancelled). Также стоит хранить дату и время последнего обновления данной информации.
* 💎 Учёт денежных средств к уплате каждому продавцу. item_id всегд состоит из 12 цифр. Первые 6 цифр у item_id — идентификатор продавца данного товара, следующие 6 цифр — идентификатор товара у данного продавца(то есть `213176086538` и `213176768102` — товары одного продавца, а `213176086538` и `133676086538` — разные товары разных продавцов, несмотря на одинаковость последних 6 цифр). Идентификатор продавца не может начинаться с 0 (минимальный идентификатор продавца `100000`). Необходимо считать продажи (delivered) каждого товара в разной валюте и их количество.
* 💎 За отсутствие ошибок подсчета при наличии 2+ партиций и 2+ инстансов сервиса. Что за ошибка может возникнуть следует догадаться самостоятельно.

### Домашнее задание №2 — ревью
Провести ревью кода.
1) Поревьюйте решение данной задачи минимум 1 сокурсника, у которого тот же тьютор, что и у вас.
2) Критерий окончания данной задачи — наличие комментариев и approve от вас на чужом merge request («апрув не глядя» мы с вами решительно осуждаем).
3) Допускаются положительные комментарии («классное решение использовать здесь вставки на ассемблере!»), вопросительные («А почему мы используем async void?»), незначительные («очепятка в названии класса»), а также значимые («Здесь лучше проверить на null reference exception», «Не стоит оставлять в коде магические числа, поэтому 49.5 сантиметров лучше вынести в отдельную константу»)

Правила ревью (базовые).
1) Будьте вежливы. Обсуждайте код, а не автора кода.
2) Не стоит использовать повелительное наклонение (исправь/измени/удоли). Предлагайте, а не приказывайте.
3) Ревьюйте так, как вы бы хотели, чтобы ревьюили вас.

Алмазиков за эту часть задания не предусмотрено.


## Docker cheat sheet
* Если у вас всё ещё нет docker — его нужно поставить.
* Запустить docker контейнеры (БД): `docker compose up -d`
* Остановить docker контейнеры (БД): `docker compose down`
* Остановить и почистить docker от данных: `docker compose down -v`
* Docker поломался: `docker system prune`


## Kafka cheat sheet
* Заводите Kafka через docker. Пример docker-compose-файла с Kafka находится в репозитории воркшопа этой недели: https://gitlab.ozon.dev/cs/classroom-9/experts/week-7/workshop-7/-/blob/master/docker-compose.yml
* Откройте ваш hosts-файл (для windows это `c:\windows\system32\drivers\etc\hosts`) Добавьте туда строчку `127.0.0.1 kafka`.
* Offset Explorer (ранее называлось Kafka Tool): https://www.kafkatool.com/ — позволяет читать и писать в Apache Kafka через простой UI. Да, писать и читать protobuf почти нереально. Но это отдельная боль в нашем мире.
* Как настроить Offset Explorer? 
* * Clusters → Add New Connection
* * Cluster Name → Любое имя
* * Вкладка Advanced → Bootstrap Servers → написать `kafka:9092`
* * Test, Add
* Серьезные девчата и пацаны качают Kafka (https://kafka.apache.org/downloads) и используют sh/bat-файлы оттуда, чтобы работать с локальной/докер/стейдж/прод Kafka. Но мы тут все несерьезные, и используем такое только в случае крайней необходимости.