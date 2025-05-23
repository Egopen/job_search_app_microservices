# WebAPI приложение по поиску работу написанный на микросервисах ASP. Net
Вся система будет представлять собой 4 микросервиса, API-шлюз и брокер сообщений RabbitMQ, написанные на языке C# фреймворк ASP Net 8.0 версия, которые будут поделены по бизнес-логике:
1.	Сервис авторизации- данный сервис будет отвечать за вход и регистрацию пользователей, в нем же будут записываться в базу данных данные о пользователе: почта, хэш пароля, рефреш токен, роль. Так же он записывает access token в куки при входе в систему
2.	Сервис соискателей- данный сервис будет отвечать за логику, связанную с соискателями: размещение резюме, просмотр резюме, изменение резюме, удаление резюме и отклик на вакансию. Все данные будут заноситься и выноситься из личной базы данных этого сервиса.
3.	Сервис работодателей- данный сервис будет отвечать за логику, связанную с работодателями: размещение, удаление, изменение, просмотр резюме, а также просмотр откликов на вакансию. Все данные будут заноситься и выноситься из личной базы данных этого сервиса.
4.	Сервис статистики- данный сервис будет собирать статистику с сервиса соискателей и работодателей: активные и неактивные резюме, их общее количество и их части от общего количества в процентах, то же самое и с вакансиями.
5.	API-шлюз-максимально простой API-шлюз написанный на ocelot, который фактически представляет собой все маршруты.
Диаграммма компонентов(в целом все связи должны идити туда-обратно, но чисто с целью удобочитаемости только односторонние связи): 

![image](https://github.com/user-attachments/assets/4ac8dfb6-c090-42f0-bb17-5302b40872ca)

## Стек
* ASP. Net
* EF
* RabbitMQ
* Ocelot

## База данных
В качестве базы данных выступает СУБД postgres
Вот диаграммы баз данных для каждого микросервиса, вполне возможно, что они уже устарели, после изменений.
1.	База данных микросервиса авторизации- данная база данных будет включать в себя две таблицы с информацией о пользователе и ролях.

![image](https://github.com/user-attachments/assets/dac25daf-93d3-435d-834e-fabb2c1aa180)

2.	База данных микросервиса соискателей- данная база данных будет содержать следующие таблицы: резюме, статусы резюме, опыт резюме и отклики резюме.
 
 ![image](https://github.com/user-attachments/assets/e04f443d-6ac0-44cc-90ee-52ad9a43ae1d)

3.	База данных микросервиса работодателей- данная база данных будет содержать следующие таблицы: таблица вакансий, статусов вакансий, таблица опыта для вакансий(0 лет, 1-3 года, 3-6 лет) и отклики на вакансию.

 ![image](https://github.com/user-attachments/assets/bf693717-40c9-4c80-a072-1dac70daef05)

4.	База данных микросервиса статистики- данная база данных будет содержать следующие таблицы: таблица статистики по вакансиям и таблица статистики по резюме. Тут я бы хотел остановиться и сказать, что так делать таблички несвязанными в целом в реляционной БД не реккомендуется, но иного выходя я не нашел.
 
 ![image](https://github.com/user-attachments/assets/e2f1e324-7d16-4ac5-abb1-b5d158803234)

## Структура проека
Каждый микросервис разделен на три слоя:
1. Controllers- папка с контроллерами.
2. Domain- папка с бизнес-логикой приложения.
3. Infrastructure- папка с БД и вспомогательными сервисами, которые очень связаны с конкретными технологиями.
Так же отдельно есть папки JSON- папки с JSON файлами, которые определяют конткракты между слоями.

Папки, которые встречаются не везде:
1. Элементы решения- папка в, которой находится docker-compose, чтобы развернуть приложение в докере, разворачивается абсолютно все.

Файлы:
1. .env- просто файл с переменными окружения.
2. Dockerfile- файл необходимы для развертывания микросервисов на докер, дефолтный файлик, который генерирует visual studio с небольшими изменнениями.

## Порты
Все порты можно посмотреть в compose, вот список:

- БД-5400
- Брокер-7000
- Шлюз-8080
- Сервис статистики-8081
- Сервис работодателя-8082
- Сервси соискателя-8083
- Сервис авторизации-8084

## Как запустить
Проще всего запустить проект в докере для этого надо скачать проект и докер, перейти в папку и сбилдить командой: 
docker-compose -p job_seeking_project up --build. После этого контейнеры соберутся и запустятся и к ним можно будет делать запросы.
## Что можно добавить/исправить
1. Раскидать JSON файлы по слоям или добавить дополнительные, чтобы четче сделать разграничения между слоями.
2. Подобавлять кастомных ошибок и дополнить обработки ошибок.
3. В целом допилить функционала: уведомления, статус просмотра отклика и многое другое.
4. Нормально покрыть тестами код(в основном я использовал постман, автотесты не писал).
## MVC проект
Это проект чисто для визуализации работы приложения, он максимально кривой и косой, но работает, в целом там можно посидеть посмотреть функционал, но анализировать код не советую.

