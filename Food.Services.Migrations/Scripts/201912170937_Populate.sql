-- Создание администратора.
--
-- Логин:  admin@food.local
-- Пароль: 123456
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (1, 'User', 'Пoльзователь', 'dc747c5f-f634-4fd8-8356-2eb54d08bd3d', false);
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (3, 'Consolidator', 'Консолидатор', '5447c91e-a863-4d97-a91a-89990ae06461', false);
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (2, 'Manager', 'Менеджер кафе', 'e5ca90cd-a838-4341-8347-8498af023bcf', false);
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (4, 'Admin', 'ВСЕСИЛЬНЫЙ АДМИН', '8dd9bed7-f475-423a-951c-5db89a719f96', false);
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (5, 'Director', 'Директор', '7887329a-9a2d-4d33-adf0-137d5b4367f7', false);
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (6, 'UserRequestToCompany', 'Запрос пользователя на привязку к компании', 'ccf388ef-02ca-45ea-8d31-7d8c456104bb', false);
INSERT INTO public.role (id, role_name, role_description, guid, is_deleted) VALUES (7, 'CompanyRequestToUser', 'Запрос компании на присоединение пользователя', '69046b5d-a02a-47e8-a3e5-938ce4508458', false);
INSERT INTO public."user" (user_access_failed_count,
                           user_email,
                           user_email_confirmed,
                           user_lockout_enabled,
                           user_lockout_enddate_utc,
                           user_password,
                           user_phone,
                           user_phone_confirmed,
                           user_security_stamp,
                           user_twofactor_enabled,
                           user_name,
                           user_fullname,
                           create_date,
                           default_address_id,
                           user_device_uuid,
                           user_display_name,
                           created_by,
                           last_upd_date,
                           last_upd_by,
                           user_first_name,
                           user_surname,
                           is_deleted,
                           percent_of_order,
                           personal_points,
                           referral_points,
                           user_referral_link,
                           sms_notify)
VALUES (0, 'admin@food.local', TRUE, FALSE, NULL, 'AAv9OTxXjoz33UsF5OoL3617FNHRTIJIDzxhatzIMnETJln/4a9fgYyVlNYzXb7o4A==', NULL, TRUE, '852c92e3-ba2d-4e49-b9f1-3c872028de2b', FALSE, 'admin@food.local', NULL, now(), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, FALSE, 0.00, 0.0000, 0.0000, '3F85C', FALSE);
INSERT INTO public."user_role_link" (user_id, role_id)
    SELECT u.id, r.id
    FROM public."user" u CROSS JOIN public."role" r
    WHERE u.user_email = 'admin@food.local' AND role_name = 'Admin'
    LIMIT 1;

INSERT INTO public.cafe_specialization (id, specialization_name, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (1, 'Пицца', null, null, null, null, false);
INSERT INTO public.cafe_specialization (id, specialization_name, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (2, 'Суши', null, null, null, null, false);
INSERT INTO public.cafe_specialization (id, specialization_name, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (3, 'Пироги', null, null, null, null, false);
INSERT INTO public.cafe_specialization (id, specialization_name, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (4, 'Шашлыки', null, null, null, null, false);
INSERT INTO public.cafe_specialization (id, specialization_name, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (5, 'Бургеры', null, null, null, null, false);
INSERT INTO public.cafe_specialization (id, specialization_name, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (6, 'Китайская еда', null, null, null, null, false);
INSERT INTO public.client (id, secret, application_type, active, refresh_token_lifetime, allowed_origin, description, is_deleted) VALUES ('nativeApp', null, 1, true, 43200, '*', 'Защищенное C# приложение', false);
INSERT INTO public.client (id, secret, application_type, active, refresh_token_lifetime, allowed_origin, description, is_deleted) VALUES ('cordovaApp', null, 0, true, 10080, '*', 'Публичное мобильное приложение', false);
INSERT INTO public.client (id, secret, application_type, active, refresh_token_lifetime, allowed_origin, description, is_deleted) VALUES ('webApp', null, 0, true, 10080, 'food.itwebnet.ru', 'Публичное Web приложение', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (1, 'Салаты', 'Салаты', 'Салаты', null, null, true, null, null, null, null, null, '4d3d44f9-653d-48e4-9cc5-d32c442369f9', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (3, 'Супы', null, null, null, null, true, null, null, null, null, null, 'c57ac27e-9583-4cbb-a61c-59f7b32988cb', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (2, 'Гарниры', null, null, null, null, true, null, null, null, null, null, '2ead6f40-419d-4c80-ada9-5c7546dabb62', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (4, 'Горячие блюда', null, null, null, null, true, null, null, null, null, null, '64b4c48b-021c-443b-bba0-a7c9c9b1cf25', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (5, 'Торты', null, null, null, null, true, null, null, null, null, null, 'b931ff15-41fa-402e-b2d4-cdfe1929f24b', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (6, 'Пицца', null, null, null, null, true, null, null, null, null, null, 'e696a5a4-54d6-4f9f-928e-fb87a14c8e52', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (7, 'Десерты', null, null, null, null, false, null, null, null, null, null, '9ae069b4-f094-4248-aaef-ea4896bd69aa', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (8, 'Выпечка', null, null, null, null, true, null, null, null, null, null, 'b1ff6c21-796d-4ac8-b56d-8aa352cde283', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (9, 'Напитки', null, null, null, null, true, null, null, null, null, null, '41eb166c-70ca-4634-98c0-f713607809a7', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (-1, 'Блюда удаленных категорий', 'Блюда удаленных категорий', 'Блюда удаленных категорий', null, null, true, null, '2015-07-23 11:20:15.730253', 5, '2015-07-23 11:20:26.188487', 5, '60475c32-9cb2-4d46-81da-eb45b2b9254f', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (10, 'Блины', 'Блины', 'Блины', null, null, true, null, '2016-05-26 09:48:05.922556', 0, null, null, 'f7918464-381d-434a-9f9d-9850b684b919', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (11, 'Завтраки', 'Завтраки', 'Завтраки', null, null, true, null, '2016-05-27 11:17:32.618835', 0, null, null, 'f301f126-c0f8-48fd-b9e3-1d9a781a5396', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (12, 'Дополнения', 'Дополнения', 'Дополнения', null, null, true, null, '2016-07-01 08:30:37.241730', 0, null, null, '863fb334-6062-464f-a6c8-a49d7640b1b0', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (14, 'Холодные закуски', 'Холодные закуски', 'Холодные закуски', null, null, true, null, '2017-12-13 11:38:16.737119', 0, null, null, 'ea4715e1-38bd-4657-940b-15435b4ff439', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (15, 'Банкетные блюда', 'Банкетные блюда', 'Банкетные блюда', null, null, true, null, '2017-12-13 11:38:42.127972', 0, null, null, 'ccb4dfb0-6f14-4d84-be0c-cee0e9f5cbbb', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (16, 'Салаты фирменные', 'Салаты фирменные', 'Салаты фирменные', null, null, true, null, '2017-12-13 11:39:01.751172', 0, null, null, '6c173ca7-ad0f-4da2-9226-cfd544c137ab', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (17, 'Роллы', 'Роллы', 'Роллы', null, null, true, null, '2017-12-13 11:39:16.429402', 0, null, null, '4cb95e51-9e56-4713-a57f-673bd4cc3ef6', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (18, 'Гриль', 'Гриль', 'Гриль', null, null, true, null, '2017-12-13 11:39:36.527697', 0, null, null, '8008ca4c-aff6-4881-aeb4-521123bf893d', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (-2, 'Удаленные блюда', 'Удаленные блюда', 'Удаленные блюда', null, null, true, null, '2015-07-23 11:20:15.730253', 5, '2015-07-23 11:20:26.188487', 5, '60475c32-9cb2-4d46-81da-eb45b2b9254f', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (19, 'Ассорти', null, null, null, null, true, null, '2019-11-20 13:10:28.095103', 0, null, null, '1e903cbb-7a89-4004-947a-6101d0c6c7a1', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (20, 'Салаты', null, null, null, null, true, null, '2019-11-20 13:10:28.095103', 0, null, null, '11473938-73f8-4ef0-9136-604dfbde8481', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (21, 'Горячие закуски, Горячее', null, null, null, null, true, null, '2019-11-20 13:10:28.095103', 0, null, null, '764482c7-6989-419d-9392-9594d3df3bb6', false);
INSERT INTO public.dish_category (id, category_name, category_full_name, category_description, category_small_image, category_big_image, is_active, parent_category_id, create_date, created_by, last_upd_date, last_upd_by, guid, is_deleted) VALUES (22, 'Фуршетные закуски', null, null, null, null, true, null, '2019-11-20 13:10:28.095103', 0, null, null, '68d2fc9c-0a24-48b6-a02b-39e509a94030', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (1, 'Европейская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (2, 'Русская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (3, 'Американская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (5, 'Японская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (6, 'Мексиканская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (7, 'Кавказская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (8, 'Узбекская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (9, 'Китайская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (10, 'Украинская', false);
INSERT INTO public.kitchen (id, kitchen_name, is_deleted) VALUES (4, 'Итальянская', false);
INSERT INTO public.log_message_codes (id, message_code, code_descr) VALUES (1, 'INFO', 'Сообщение с информацией');
INSERT INTO public.log_message_codes (id, message_code, code_descr) VALUES (2, 'DEBUG', 'Отладочное сообщение');
INSERT INTO public.log_message_codes (id, message_code, code_descr) VALUES (3, 'ERROR', 'Сообщение об ошибке');
INSERT INTO public.notification_channel (id, notification_channel_code, notification_channel_name, is_deleted) VALUES (1, 'EMAIL', 'Отправка сообщения на email', false);
INSERT INTO public.notification_type (id, notification_type_code, notification_type_name, is_deleted) VALUES (1, 'USER_REG', 'Регистрация нового пользователя', false);
INSERT INTO public.notification_type (id, notification_type_code, notification_type_name, is_deleted) VALUES (2, 'ORDER_CREATE', 'Создание нового заказа', false);
INSERT INTO public.notification_type (id, notification_type_code, notification_type_name, is_deleted) VALUES (3, 'ORDER_CANCEL', 'Заказ отменен', false);
INSERT INTO public.object_type (id, description, guid, is_deleted) VALUES (1, 'CAFE', '4ea155ab-d275-471f-ad8e-8f126da0a169', false);
INSERT INTO public.object_type (id, description, guid, is_deleted) VALUES (2, 'CATEGORY', '4ab14c6d-afdd-4ec0-b31a-e381afda4c21', false);
INSERT INTO public.object_type (id, description, guid, is_deleted) VALUES (3, 'DISH', 'f2fbc262-6f1a-4884-9357-36fac7929b1c', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ в процессе доставки пользователю', 'DELIVERY', 4, 'c2f4b1ab-7b9b-4156-8319-9c1746d61c2e', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ отменен', 'ABORT', 5, 'b9cbc822-33c1-4cbf-abd9-4931566dca27', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ на компанию сформирован, ожидание обработки заказа в кафе', 'COMP_READY', 6, 'bffe81c1-6c7d-4f15-b7b8-0d31152b38c3', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ требует уточнения какого-нибудь вопроса у пользователя', 'QUESTION', 7, '62751366-aeb0-481e-9108-db560ceb9ad3', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ принят кафе в обработку, приготовление блюда', 'CAFE_PROCE', 8, '4c25e14b-5bc0-4769-935a-d6a4e0cd96db', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ успешно выполнен в кафе, можно отдавать в доставку', 'CAFE_COMPL', 9, 'c80dd119-5fda-4c14-971d-725f10553470', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ отменён пользователем', 'ABORT_USER', 10, 'd7169ee8-9b5f-4abb-8421-ade02436db96', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ отменён кафе', 'ABORT_CAFE', 11, '1a406ddf-ed01-4bd4-8121-37633291b360', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ отменён доставщиком', 'ABORT_DELI', 12, '5b449bba-de45-44d5-89b0-08f7192ce16f', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ на компаниюؠоткрыт и сотрудники могут делать свои заказы', 'COMP_OPEN', 2, 'c94a6fce-045e-4420-bd7d-5d1e5519ff7a', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ является пользовательской корзиной сайта', 'CART', 13, 'c44ca443-3b12-4842-a10e-6bbdb1a1797c', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Заказ успешно выполнен и закрыт', 'NEW1', 1, 'bfd740f3-41eb-4ab3-b984-3b813dabcc7a', false);
INSERT INTO public.order_status (status_name, status_code, id, guid, is_deleted) VALUES ('Сформированный пользователем новый заказ', 'NEW', 3, 'b1f18786-d55a-4af3-84ef-35ddbce4baad', false);
INSERT INTO public.reports_extensions (id, ext, create_date, created_by, last_upd_date, last_upd_by, is_deleted) VALUES (1, 'html', '2016-07-01 11:37:46.004124', 0, null, null, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Сладкое', null, '2016-04-28 17:10:17.438469', 0, null, null, 14, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('С грибами', null, '2016-04-28 17:10:05.305049', 0, null, null, 13, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Блюда с курицей', null, '2016-04-28 17:09:40.909718', 0, null, null, 12, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Блюда с рыбой', null, '2016-04-28 17:09:29.075423', 0, null, null, 11, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Напитки', null, '2016-04-28 17:08:09.379894', 0, null, null, 9, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Детское', null, '2016-04-28 17:08:04.178949', 0, null, null, 8, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Популярное', null, '2016-04-28 17:06:09.760705', 0, null, null, 7, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Мясное', null, '2016-04-28 17:05:44.732335', 0, null, null, 6, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Постное', null, '2016-04-28 17:04:32.566806', 0, null, null, 5, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Супы', null, '2016-04-24 17:56:02.714335', 0, null, null, 2, false, false);
INSERT INTO public.tags (name, parent_id, create_date, created_by, last_upd, last_upd_by, id, is_active, is_deleted) VALUES ('Салаты', null, '2016-04-24 17:55:56.855981', 0, null, null, 1, false, false);
