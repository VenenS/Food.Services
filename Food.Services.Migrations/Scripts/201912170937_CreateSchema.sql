--
-- PostgreSQL database dump
--

-- Dumped from database version 9.5.18
-- Dumped by pg_dump version 11.5

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;
SET default_tablespace = '';
SET default_with_oids = false;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" SCHEMA public;

--
-- Name: actions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.actions (
    id bigint NOT NULL,
    action_name character varying(150),
    points numeric(15,4) DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_by bigint,
    last_upd_date timestamp(6) without time zone
);


--
-- Name: TABLE actions; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.actions IS 'Акции, за которые начисляют баллы';


--
-- Name: COLUMN actions.action_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.actions.action_name IS 'Название акции';


--
-- Name: COLUMN actions.points; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.actions.points IS 'Количество баллов за акцию';


--
-- Name: COLUMN actions.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.actions.create_date IS 'Дата создания записи';


--
-- Name: COLUMN actions.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.actions.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN actions.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.actions.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN actions.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.actions.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: actions_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.actions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: actions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.actions_id_seq OWNED BY public.actions.id;


--
-- Name: address_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.address_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: address; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.address (
    id bigint DEFAULT nextval('public.address_id_seq'::regclass) NOT NULL,
    city_id bigint,
    street_id bigint,
    house_id bigint,
    city_offname character varying(120),
    street_offname character varying(120),
    housenum character varying(10),
    buildnum character varying(10),
    flat character varying(128),
    office character varying(128),
    extrainfo character varying(512),
    postalcode character varying(6),
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by bigint,
    raw_address character varying(4096),
    address_comment text,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE address; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.address IS 'Таблица адресов';


--
-- Name: COLUMN address.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.id IS 'Идентификатор адреса';


--
-- Name: COLUMN address.city_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.city_id IS 'Идентификатор города';


--
-- Name: COLUMN address.street_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.street_id IS 'Идентификатор улицы';


--
-- Name: COLUMN address.house_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.house_id IS 'Идентификатор дома';


--
-- Name: COLUMN address.city_offname; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.city_offname IS 'Название города';


--
-- Name: COLUMN address.street_offname; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.street_offname IS 'Название улицы';


--
-- Name: COLUMN address.housenum; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.housenum IS 'Номер дома в текстовом виде';


--
-- Name: COLUMN address.buildnum; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.buildnum IS 'Номер строения в текстовом виде';


--
-- Name: COLUMN address.flat; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.flat IS 'Номер квартиры в текстовом виде';


--
-- Name: COLUMN address.office; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.office IS 'Номер офиса в текстовом виде';


--
-- Name: COLUMN address.extrainfo; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.extrainfo IS 'Дополнительная информация по адресу';


--
-- Name: COLUMN address.postalcode; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.postalcode IS 'Почтовый индекс';


--
-- Name: COLUMN address.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.create_date IS 'Дата создания записи';


--
-- Name: COLUMN address.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN address.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN address.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.last_upd_by IS 'Пользователь, последний изменивший запись.';


--
-- Name: COLUMN address.raw_address; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address.raw_address IS 'Не форматированный адрес';


--
-- Name: address_company_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.address_company_link (
    id bigint NOT NULL,
    company_id bigint,
    address_id bigint,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    display_type integer DEFAULT 0 NOT NULL
);


--
-- Name: TABLE address_company_link; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.address_company_link IS 'Таблица связи компании и адреса';


--
-- Name: COLUMN address_company_link.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address_company_link.create_date IS 'Дата создания записи';


--
-- Name: COLUMN address_company_link.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address_company_link.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN address_company_link.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address_company_link.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN address_company_link.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address_company_link.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN address_company_link.is_deleted; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.address_company_link.is_deleted IS 'Флаг логического удаления';


--
-- Name: address_company_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.address_company_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: address_company_link_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.address_company_link_id_seq OWNED BY public.address_company_link.id;


--
-- Name: bankets_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.bankets_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: bankets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.bankets (
    id bigint DEFAULT nextval('public.bankets_id_seq'::regclass) NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    event_date timestamp without time zone DEFAULT now() NOT NULL,
    order_start_date timestamp without time zone DEFAULT now() NOT NULL,
    order_end_date timestamp without time zone DEFAULT now() NOT NULL,
    menu_id bigint NOT NULL,
    company_id bigint NOT NULL,
    cafe_id bigint NOT NULL,
    total_sum numeric(15,2) DEFAULT 0 NOT NULL
);


--
-- Name: cafe_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe (
    id bigint DEFAULT nextval('public.cafe_id_seq'::regclass) NOT NULL,
    cafe_name character varying(50),
    cafe_full_name character varying(256),
    cafe_description character varying(4096),
    cafe_short_description character varying(256),
    cafe_small_image character varying(50),
    cafe_big_image character varying(50),
    is_active boolean DEFAULT true NOT NULL,
    last_upd_date timestamp without time zone,
    cafe_specialization_id bigint,
    minimum_order_rub numeric(15,2) DEFAULT 0,
    delivery_price_rub numeric(15,2) DEFAULT 0,
    online_payment_sign boolean,
    average_delivery_time integer,
    delivery_comment character varying(1024),
    working_week_days character varying(30),
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_by bigint,
    guid uuid DEFAULT public.uuid_generate_v4(),
    cafe_rating_sum bigint DEFAULT 0,
    cafe_rating_count bigint DEFAULT 0,
    business_hours character varying(8192),
    cafe_user_type character varying(50) DEFAULT 'PERSON_ONLY'::character varying NOT NULL,
    clean_url_name character varying(256),
    is_deleted boolean DEFAULT false NOT NULL,
    allow_payment_by_points boolean DEFAULT false,
    week_menu_is_active boolean DEFAULT false NOT NULL,
    address character varying(120),
    phone character varying(64),
    deferred_order boolean DEFAULT false NOT NULL,
    logo text,
    daily_corp_order_sum numeric(15,2) DEFAULT 0
);


--
-- Name: TABLE cafe; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe IS 'Кафе, зарегистрированные в системе';


--
-- Name: COLUMN cafe.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.id IS 'Идентификатор кафе';


--
-- Name: COLUMN cafe.cafe_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_name IS 'Название кафе';


--
-- Name: COLUMN cafe.cafe_full_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_full_name IS 'Полное название кафе';


--
-- Name: COLUMN cafe.cafe_description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_description IS 'Полное описание кафе';


--
-- Name: COLUMN cafe.cafe_short_description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_short_description IS 'Краткое описание кафе';


--
-- Name: COLUMN cafe.cafe_small_image; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_small_image IS 'Маленький логотип кафе';


--
-- Name: COLUMN cafe.cafe_big_image; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_big_image IS 'Большое изображение логотипа кафе';


--
-- Name: COLUMN cafe.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.is_active IS 'Признак активности кафе';


--
-- Name: COLUMN cafe.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.last_upd_date IS 'Дата последнего обновления кафе';


--
-- Name: COLUMN cafe.cafe_specialization_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_specialization_id IS 'Идентификатор специализации кафе';


--
-- Name: COLUMN cafe.minimum_order_rub; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.minimum_order_rub IS 'Минимальная сумма заказа, в рублях';


--
-- Name: COLUMN cafe.delivery_price_rub; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.delivery_price_rub IS 'Минимальная сумма доставки, в рублях';


--
-- Name: COLUMN cafe.online_payment_sign; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.online_payment_sign IS 'Возможна ли онлайн оплата на сайте';


--
-- Name: COLUMN cafe.average_delivery_time; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.average_delivery_time IS 'Среднее время доставки, в минутах';


--
-- Name: COLUMN cafe.working_week_days; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.working_week_days IS 'Список рабочих дней в неделе, разделенный запятыми. Пример:1,2,3,4,5
Значение NULL означает, что работает без выходных.';


--
-- Name: COLUMN cafe.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.create_date IS 'Дата создания записи о кафе';


--
-- Name: COLUMN cafe.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.created_by IS 'Идентификатор пользователя, создавшего запись';


--
-- Name: COLUMN cafe.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.last_upd_by IS 'Идентификатор пользователя, который провел последнее изменение записи';


--
-- Name: COLUMN cafe.cafe_rating_sum; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_rating_sum IS 'Сумма всех рейтингов по данному блюду';


--
-- Name: COLUMN cafe.cafe_rating_count; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_rating_count IS 'Количество всех рейтингов по данному блюду';


--
-- Name: COLUMN cafe.cafe_user_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.cafe_user_type IS 'Признак для определения типа работы с пользователями

Допустимые значения:
COMPANY_ONLY - кафе принимает только компанейский заказ
COMPANY_PERSON - кафе работает и по компанейским заказам и по персональным
PERSON_ONLY - кафе работает только по персональным заказам

Дефолтовое значение PERSON_ONLY';


--
-- Name: COLUMN cafe.logo; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe.logo IS 'Логотип кафе - маленькая картинка 48х48 в Base64';


--
-- Name: cafe_category_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_category_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_category_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_category_link (
    cafe_id bigint,
    category_id bigint,
    is_active boolean DEFAULT true NOT NULL,
    id bigint DEFAULT nextval('public.cafe_category_link_id_seq'::regclass) NOT NULL,
    category_index integer,
    create_date timestamp without time zone DEFAULT now(),
    created_by integer DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE cafe_category_link; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe_category_link IS 'Таблицы связи кафе и категорий';


--
-- Name: COLUMN cafe_category_link.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.cafe_id IS 'Идентификатор кафе';


--
-- Name: COLUMN cafe_category_link.category_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.category_id IS 'Идентификатор категории';


--
-- Name: COLUMN cafe_category_link.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.is_active IS 'Признак активности записи';


--
-- Name: COLUMN cafe_category_link.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.id IS 'Идентификатор записи';


--
-- Name: COLUMN cafe_category_link.category_index; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.category_index IS 'Порядок, в котором будут отображаться выбранные категории в меню';


--
-- Name: COLUMN cafe_category_link.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.create_date IS 'Дата создания записи';


--
-- Name: COLUMN cafe_category_link.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN cafe_category_link.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN cafe_category_link.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_category_link.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: cafe_discount; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_discount (
    id bigint NOT NULL,
    company_id bigint,
    cafe_id bigint NOT NULL,
    summ_from numeric(15,2) NOT NULL,
    summ_to numeric(15,2),
    percent numeric,
    summ numeric(15,2),
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    discount_begin_date date NOT NULL,
    discount_end_date date
);


--
-- Name: COLUMN cafe_discount.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_discount.id IS 'Идентификатор записи';


--
-- Name: COLUMN cafe_discount.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_discount.create_date IS 'Дата создания записи';


--
-- Name: COLUMN cafe_discount.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_discount.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN cafe_discount.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_discount.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN cafe_discount.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_discount.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: cafe_discount_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_discount_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_discount_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.cafe_discount_id_seq OWNED BY public.cafe_discount.id;


--
-- Name: cafe_kitchen_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_kitchen_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_kitchen_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_kitchen_link (
    id bigint DEFAULT nextval('public.cafe_kitchen_link_id_seq'::regclass) NOT NULL,
    cafe_id bigint,
    kitchen_id bigint,
    create_date timestamp without time zone DEFAULT now(),
    created_by integer DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE cafe_kitchen_link; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe_kitchen_link IS 'Таблица связи национальных кухонь и кафе';


--
-- Name: COLUMN cafe_kitchen_link.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.id IS 'Идентификатор связи кафе/кухня';


--
-- Name: COLUMN cafe_kitchen_link.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.cafe_id IS 'Идентификатор кафе';


--
-- Name: COLUMN cafe_kitchen_link.kitchen_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.kitchen_id IS 'Идентификатор национальной кухни';


--
-- Name: COLUMN cafe_kitchen_link.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.create_date IS 'Дата создания записи';


--
-- Name: COLUMN cafe_kitchen_link.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN cafe_kitchen_link.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN cafe_kitchen_link.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_kitchen_link.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: cafe_managers_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_managers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_managers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_managers (
    id bigint DEFAULT nextval('public.cafe_managers_id_seq'::regclass) NOT NULL,
    user_id bigint,
    cafe_id bigint,
    user_role character varying(30),
    create_date timestamp without time zone DEFAULT now(),
    created_by integer DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE cafe_managers; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe_managers IS 'Список менеджеров в кафе';


--
-- Name: COLUMN cafe_managers.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.id IS 'Идентификатор записи';


--
-- Name: COLUMN cafe_managers.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.user_id IS 'Идентификатор пользователя из таблицы user';


--
-- Name: COLUMN cafe_managers.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.cafe_id IS 'Идентификатор кафе';


--
-- Name: COLUMN cafe_managers.user_role; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.user_role IS 'Название роли пользователя в данном кафе';


--
-- Name: COLUMN cafe_managers.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.create_date IS 'Дата создания записи';


--
-- Name: COLUMN cafe_managers.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN cafe_managers.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN cafe_managers.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_managers.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: cafe_menu_patterns_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_menu_patterns_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_menu_patterns; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_menu_patterns (
    id bigint DEFAULT nextval('public.cafe_menu_patterns_id_seq'::regclass) NOT NULL,
    is_deleted boolean DEFAULT false,
    cafe_id bigint NOT NULL,
    name character varying(128) NOT NULL,
    pattern_to_date timestamp without time zone DEFAULT now(),
    is_banket boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE cafe_menu_patterns; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe_menu_patterns IS 'Шаблоны меню кафе';


--
-- Name: cafe_menu_patterns_dishes_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_menu_patterns_dishes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_menu_patterns_dishes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_menu_patterns_dishes (
    id bigint DEFAULT nextval('public.cafe_menu_patterns_dishes_id_seq'::regclass) NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    pattern_id bigint NOT NULL,
    dish_id bigint NOT NULL,
    price numeric(15,2) DEFAULT 0,
    name character varying(128)
);


--
-- Name: cafe_notification_contacts_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_notification_contacts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_notification_contacts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_notification_contacts (
    id bigint DEFAULT nextval('public.cafe_notification_contacts_id_seq'::regclass) NOT NULL,
    cafe_id bigint,
    notification_channel_id bigint,
    notification_contact character varying(256),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE cafe_notification_contacts; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe_notification_contacts IS 'Таблица с контактами кафе. Для одного кафе может быть несколько email, контактных телефонов для отправки смс.';


--
-- Name: COLUMN cafe_notification_contacts.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_notification_contacts.id IS 'Идентификатор контакт кафе';


--
-- Name: COLUMN cafe_notification_contacts.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_notification_contacts.cafe_id IS 'Идентификатор кафе';


--
-- Name: COLUMN cafe_notification_contacts.notification_channel_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_notification_contacts.notification_channel_id IS 'Идентификатор канала уведомлений (из таблицы notification_channel)';


--
-- Name: COLUMN cafe_notification_contacts.notification_contact; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_notification_contacts.notification_contact IS 'Наименование адреса доставки уведомления (example@example.com для email, +79128219999 для телефона)';


--
-- Name: cafe_order_notification; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_order_notification (
    id bigint NOT NULL,
    cafe_id bigint NOT NULL,
    user_id bigint NOT NULL,
    deliver_datetime timestamp without time zone DEFAULT now()
);


--
-- Name: cafe_order_notification_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_order_notification_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_order_notification_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.cafe_order_notification_id_seq OWNED BY public.cafe_order_notification.cafe_id;


--
-- Name: cafe_order_notifications_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_order_notifications_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_order_notifications_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.cafe_order_notifications_id_seq OWNED BY public.cafe_order_notification.id;


--
-- Name: cafe_order_notifications_user_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_order_notifications_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_order_notifications_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.cafe_order_notifications_user_id_seq OWNED BY public.cafe_order_notification.user_id;


--
-- Name: cafe_specialization_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cafe_specialization_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cafe_specialization; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cafe_specialization (
    id bigint DEFAULT nextval('public.cafe_specialization_id_seq'::regclass) NOT NULL,
    specialization_name character varying(50),
    create_date timestamp without time zone DEFAULT now(),
    created_by integer DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE cafe_specialization; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cafe_specialization IS 'Таблица наименований специализации кафе (Пицца, Пироги, Суши и т.д.)';


--
-- Name: COLUMN cafe_specialization.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_specialization.id IS 'Идентификатор специализации кафе';


--
-- Name: COLUMN cafe_specialization.specialization_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_specialization.specialization_name IS 'Наименование специализации кафе (Пицца, Пироги, Суши и т.д.)';


--
-- Name: COLUMN cafe_specialization.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_specialization.create_date IS 'Дата создания записи';


--
-- Name: COLUMN cafe_specialization.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_specialization.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN cafe_specialization.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_specialization.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN cafe_specialization.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cafe_specialization.last_upd_by IS 'Пользователь, последний изменивший запись.';


--
-- Name: client; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.client (
    id character varying(100) NOT NULL,
    secret character varying,
    application_type bigint,
    active boolean,
    refresh_token_lifetime bigint,
    allowed_origin character varying,
    description character varying,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: client_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.client_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.company_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.company (
    id bigint DEFAULT nextval('public.company_id_seq'::regclass) NOT NULL,
    company_name character varying(256),
    company_full_name character varying(1024),
    company_jur_address_id bigint,
    company_post_address_id bigint,
    main_delivery_address_id bigint,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_active boolean DEFAULT true NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL,
    sms_notify boolean DEFAULT false
);


--
-- Name: TABLE company; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.company IS 'Список компаний, которые заказывают обеды своим сотрудникам';


--
-- Name: COLUMN company.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.id IS 'Идентификатор записи';


--
-- Name: COLUMN company.company_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.company_name IS 'Название компании';


--
-- Name: COLUMN company.company_full_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.company_full_name IS 'Полное название компании';


--
-- Name: COLUMN company.company_jur_address_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.company_jur_address_id IS 'Идентификатор юридического адреса органзации';


--
-- Name: COLUMN company.company_post_address_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.company_post_address_id IS 'Идентификатор почтового адреса органзации';


--
-- Name: COLUMN company.main_delivery_address_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.main_delivery_address_id IS 'Основной адрес доставки для данной организации';


--
-- Name: COLUMN company.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.create_date IS 'Дата создания записи';


--
-- Name: COLUMN company.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN company.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN company.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN company.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.is_active IS 'Флаг активности компании';


--
-- Name: COLUMN company.sms_notify; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company.sms_notify IS 'Включает/отключает отправку СМС-оповещений по корпоративным заказам для пользователей компании';


--
-- Name: company_curators_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.company_curators_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company_curators; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.company_curators (
    id bigint DEFAULT nextval('public.company_curators_id_seq'::regclass) NOT NULL,
    user_id bigint NOT NULL,
    company_id bigint NOT NULL,
    create_date timestamp without time zone DEFAULT now(),
    created_by integer DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: company_order_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.company_order_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company_order; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.company_order (
    id bigint DEFAULT nextval('public.company_order_id_seq'::regclass) NOT NULL,
    company_id bigint,
    order_create_date timestamp without time zone,
    order_open_date timestamp without time zone,
    order_auto_close_date timestamp without time zone,
    order_delivery_address bigint,
    total_price numeric(15,2) DEFAULT 0,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    order_delivery_date timestamp without time zone,
    cafe_id bigint,
    state bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    contact_email character varying(256) DEFAULT NULL::character varying,
    contact_phone text,
    total_delivery_cost numeric(15,2) DEFAULT 0.0 NOT NULL
);


--
-- Name: TABLE company_order; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.company_order IS 'Таблица с заказами, осуществленными организацией';


--
-- Name: COLUMN company_order.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.id IS 'Идентификатор записи';


--
-- Name: COLUMN company_order.company_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.company_id IS 'Идентификатор компании';


--
-- Name: COLUMN company_order.order_create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.order_create_date IS 'Дата создания заказа';


--
-- Name: COLUMN company_order.order_open_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.order_open_date IS 'Дата начала приема заказов от сотрудников данной организации по данному заказу.';


--
-- Name: COLUMN company_order.order_auto_close_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.order_auto_close_date IS 'Дата автоматического закрытия возможности заказа.';


--
-- Name: COLUMN company_order.order_delivery_address; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.order_delivery_address IS 'Адрес доставки заказа';


--
-- Name: COLUMN company_order.total_price; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.total_price IS 'Общая сумма заказа';


--
-- Name: COLUMN company_order.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.create_date IS 'Дата создания записи';


--
-- Name: COLUMN company_order.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN company_order.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN company_order.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.last_upd_by IS 'Пользователь, последний изменивший запись.';


--
-- Name: COLUMN company_order.order_delivery_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.order_delivery_date IS 'Дата доставки заказа в компанию';


--
-- Name: COLUMN company_order.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.cafe_id IS 'Идентификатор кафе';


--
-- Name: COLUMN company_order.total_delivery_cost; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order.total_delivery_cost IS 'Общая стоимость доставки корпоративного заказа';


--
-- Name: company_order_schedule_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.company_order_schedule_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company_order_schedule; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.company_order_schedule (
    id bigint DEFAULT nextval('public.company_order_schedule_id_seq'::regclass) NOT NULL,
    company_id bigint,
    company_delivery_address_id bigint,
    is_active boolean DEFAULT true NOT NULL,
    schedule_begin_date date,
    schedule_end_date date,
    order_start_time time without time zone,
    order_stop_date time without time zone,
    order_send_time time without time zone,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by integer,
    cafe_id bigint,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE company_order_schedule; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.company_order_schedule IS 'Таблица с расписаниями автоматического создания заказов на компанию';


--
-- Name: COLUMN company_order_schedule.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.id IS 'Идентификатор расписания заказа';


--
-- Name: COLUMN company_order_schedule.company_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.company_id IS 'Идентификатор компании, для которой создается расписание создания заказов';


--
-- Name: COLUMN company_order_schedule.company_delivery_address_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.company_delivery_address_id IS 'Идентификатор адреса, по которому будет совершена доставка данного заказа';


--
-- Name: COLUMN company_order_schedule.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.is_active IS 'Признак активности расписания';


--
-- Name: COLUMN company_order_schedule.schedule_begin_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.schedule_begin_date IS 'Дата начала действия данного расписания';


--
-- Name: COLUMN company_order_schedule.schedule_end_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.schedule_end_date IS 'Дата окончания действия данного расписания';


--
-- Name: COLUMN company_order_schedule.order_start_time; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.order_start_time IS 'Дата со временем начала формирования заказа. Если пусто, то заказ можно начинать формировать с 0 часов текущего дня.';


--
-- Name: COLUMN company_order_schedule.order_stop_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.order_stop_date IS 'Время окончания приема заказов от сотрудников компании по данному заказу. Если пустое, то отправка заказа будет осуществляться вручную.';


--
-- Name: COLUMN company_order_schedule.order_send_time; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.order_send_time IS 'Время отправки заказа в кафе. Если пусто - то отправка заказа осуществляется вручную.';


--
-- Name: COLUMN company_order_schedule.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.create_date IS 'Дата создания записи';


--
-- Name: COLUMN company_order_schedule.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN company_order_schedule.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN company_order_schedule.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.last_upd_by IS 'Пользователь, последний изменивший запись.';


--
-- Name: COLUMN company_order_schedule.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.company_order_schedule.cafe_id IS 'Идентификатор кафе, в котором будет создаваться заказ';


--
-- Name: company_order_status_history; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.company_order_status_history (
    id bigint NOT NULL,
    company_order_id bigint,
    order_status integer,
    created_by bigint DEFAULT 0,
    create_date timestamp(6) without time zone DEFAULT now(),
    last_upd_by bigint,
    last_upd_date timestamp(6) without time zone,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: company_order_status_history_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.company_order_status_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company_order_status_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.company_order_status_history_id_seq OWNED BY public.company_order_status_history.id;


--
-- Name: cost_of_delivery; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cost_of_delivery (
    id bigint NOT NULL,
    cafe_id bigint,
    order_price_from numeric(15,2) DEFAULT 0,
    order_price_to numeric(15,2) DEFAULT 0,
    delivery_price numeric(15,2) DEFAULT 0,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    for_company_orders boolean DEFAULT true NOT NULL
);


--
-- Name: TABLE cost_of_delivery; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.cost_of_delivery IS 'Таблица определения стоимости доставки';


--
-- Name: COLUMN cost_of_delivery.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.id IS 'Идентификатор записи';


--
-- Name: COLUMN cost_of_delivery.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.cafe_id IS 'Идентификатор кафе';


--
-- Name: COLUMN cost_of_delivery.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.create_date IS 'Дата создания записи';


--
-- Name: COLUMN cost_of_delivery.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN cost_of_delivery.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN cost_of_delivery.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN cost_of_delivery.for_company_orders; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.cost_of_delivery.for_company_orders IS 'Тип заказов, для которы определяется стоимость доставки: true - для корпоративных, false - для персональных';


--
-- Name: cost_of_delivery_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.cost_of_delivery_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: cost_of_delivery_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.cost_of_delivery_id_seq OWNED BY public.cost_of_delivery.id;


--
-- Name: discount_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.discount_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: discount; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.discount (
    id bigint DEFAULT nextval('public.discount_id_seq'::regclass) NOT NULL,
    user_id bigint,
    company_id bigint,
    discount numeric,
    discount_begin_date date,
    discount_end_date date,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by bigint,
    cafe_id bigint,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE discount; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.discount IS 'Таблица с правилами применения скидок';


--
-- Name: COLUMN discount.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.id IS 'Идентификатор правила скидки';


--
-- Name: COLUMN discount.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.user_id IS 'Идентификатор пользователя';


--
-- Name: COLUMN discount.company_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.company_id IS 'Идентификатор компании';


--
-- Name: COLUMN discount.discount; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.discount IS 'Размер скидки в процентах';


--
-- Name: COLUMN discount.discount_begin_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.discount_begin_date IS 'Дата начала действия скидки';


--
-- Name: COLUMN discount.discount_end_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.discount_end_date IS 'Дата окончания действия скидки';


--
-- Name: COLUMN discount.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.create_date IS 'Дата создания записи';


--
-- Name: COLUMN discount.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN discount.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.last_upd_date IS 'Дата последнего обновления записи';


--
-- Name: COLUMN discount.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN discount.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.discount.cafe_id IS 'Идентификатор кафе, которое предоставляет скидку';


--
-- Name: dish; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dish (
    id bigint NOT NULL,
    cafe_category_link_id bigint DEFAULT 0 NOT NULL,
    food_dish_name character varying(100),
    kcalories numeric,
    weight numeric,
    weight_description character(100),
    image_id character varying(50),
    is_active boolean DEFAULT true NOT NULL,
    version_from date DEFAULT '2000-01-01'::date,
    version_to date DEFAULT '2099-12-31'::date,
    base_price_rub numeric(15,2) DEFAULT 0,
    food_dish_index integer,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL,
    guid uuid DEFAULT public.uuid_generate_v4(),
    dish_rating_sum bigint DEFAULT 0,
    dish_rating_count bigint DEFAULT 0,
    description text,
    composition text
);


--
-- Name: TABLE dish; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.dish IS 'Таблица с блюдами';


--
-- Name: COLUMN dish.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.id IS 'Идентификатор блюда';


--
-- Name: COLUMN dish.cafe_category_link_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.cafe_category_link_id IS 'Идентификатор категории блюда в данном кафе';


--
-- Name: COLUMN dish.food_dish_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.food_dish_name IS 'Название блюда';


--
-- Name: COLUMN dish.kcalories; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.kcalories IS 'Килокалории';


--
-- Name: COLUMN dish.weight; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.weight IS 'Вес блюда, грамм';


--
-- Name: COLUMN dish.weight_description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.weight_description IS 'Развесовка, например 100/30/10/60. В текстовом виде.';


--
-- Name: COLUMN dish.image_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.image_id IS 'Идентификатор изображения
';


--
-- Name: COLUMN dish.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.is_active IS 'Признак активности блюда';


--
-- Name: COLUMN dish.version_from; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.version_from IS 'Дата начала действия данной версии блюда';


--
-- Name: COLUMN dish.version_to; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.version_to IS 'Дата окончания действия данной версии блюда.';


--
-- Name: COLUMN dish.base_price_rub; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.base_price_rub IS 'Базовая цена блюда';


--
-- Name: COLUMN dish.food_dish_index; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.food_dish_index IS 'Порядок вывода блюда в рамках категории';


--
-- Name: COLUMN dish.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.create_date IS 'Дата создания записи';


--
-- Name: COLUMN dish.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN dish.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN dish.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: dish_category; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dish_category (
    id bigint NOT NULL,
    category_name character varying(50),
    category_full_name character varying(100),
    category_description character varying(1024),
    category_small_image character varying(30),
    category_big_image character varying(30),
    is_active boolean DEFAULT true NOT NULL,
    parent_category_id bigint,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    guid uuid DEFAULT public.uuid_generate_v4(),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE dish_category; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.dish_category IS 'Категории блюд, предлагаемых в кафе (салаты, горячее, супы и т.д.)';


--
-- Name: COLUMN dish_category.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.id IS 'Идентификатор категории';


--
-- Name: COLUMN dish_category.category_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.category_name IS 'Наименование категории';


--
-- Name: COLUMN dish_category.category_full_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.category_full_name IS 'Полное наименование категории';


--
-- Name: COLUMN dish_category.category_description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.category_description IS 'Описание категории';


--
-- Name: COLUMN dish_category.category_small_image; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.category_small_image IS 'Маленькое изобрежение категории';


--
-- Name: COLUMN dish_category.category_big_image; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.category_big_image IS 'Большое изображение категории';


--
-- Name: COLUMN dish_category.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.is_active IS 'Признак активности категории';


--
-- Name: COLUMN dish_category.parent_category_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.parent_category_id IS 'Родительская категория';


--
-- Name: COLUMN dish_category.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.create_date IS 'Дата создания записи';


--
-- Name: COLUMN dish_category.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN dish_category.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN dish_category.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_category.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: food_schedule_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.food_schedule_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: dish_in_menu; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dish_in_menu (
    id bigint DEFAULT nextval('public.food_schedule_id_seq'::regclass) NOT NULL,
    dish_id bigint,
    s_type character(1),
    is_active boolean DEFAULT true NOT NULL,
    schedule_begin_date date,
    schedule_end_date date,
    s_month_day character(100),
    s_week_day character(30),
    food_price_rub numeric(15,2),
    one_time_date date,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE dish_in_menu; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.dish_in_menu IS 'Представляет блюдо в меню.';


--
-- Name: COLUMN dish_in_menu.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.id IS 'Идентификатор расписаний';


--
-- Name: COLUMN dish_in_menu.dish_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.dish_id IS 'Идентификатор блюда';


--
-- Name: COLUMN dish_in_menu.s_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.s_type IS 'Тип расписания:
D - daily (блюдо доступно ежедневно),
W - weekly (блюдо доступно в определенные дни недели),
M - monthly (блюдо доступно в определенные дни месяца)
S - simple (one time run)';


--
-- Name: COLUMN dish_in_menu.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.is_active IS 'Признак активности блюда';


--
-- Name: COLUMN dish_in_menu.schedule_begin_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.schedule_begin_date IS 'Дата начала действия данного расписания';


--
-- Name: COLUMN dish_in_menu.schedule_end_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.schedule_end_date IS 'Дата окончания действия данного расписания';


--
-- Name: COLUMN dish_in_menu.s_month_day; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.s_month_day IS 'День месяца, в который действует данное расписание. Дни месяца разделены запятой. Например:
1,7,14,21,28
Первый день месяца начинается с 1.';


--
-- Name: COLUMN dish_in_menu.s_week_day; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.s_week_day IS 'День недели, в который действует данное расписание. Дни месяца разделены запятой. Например:
1,3,5
Первый день недели начинается с 1.';


--
-- Name: COLUMN dish_in_menu.food_price_rub; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.food_price_rub IS 'Стоимость блюда в меню';


--
-- Name: COLUMN dish_in_menu.one_time_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.one_time_date IS 'Единичная дата включения/исключения в меню. Если стоит s_type =''S'' или ''E'', то они будут действовать только для указанной в этой поле даты.';


--
-- Name: COLUMN dish_in_menu.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.last_upd_date IS 'Дата последнего обновления записи
';


--
-- Name: COLUMN dish_in_menu.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN dish_in_menu.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.create_date IS 'Дата создания записи';


--
-- Name: COLUMN dish_in_menu.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu.created_by IS 'Пользователь создавший запись';


--
-- Name: dish_in_menu_history; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dish_in_menu_history (
    dish_id bigint NOT NULL,
    user_id bigint NOT NULL,
    last_upd_date timestamp without time zone,
    s_type character varying(1) NOT NULL,
    price numeric(15,2) NOT NULL,
    id bigint NOT NULL
);


--
-- Name: COLUMN dish_in_menu_history.s_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_in_menu_history.s_type IS 'Тип расписания:
D - daily (блюдо доступно ежедневно),
W - weekly (блюдо доступно в определенные дни недели),
M - monthly (блюдо доступно в определенные дни месяца)
S - simple (one time run)';


--
-- Name: dish_in_menu_history_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.dish_in_menu_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: dish_in_menu_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.dish_in_menu_history_id_seq OWNED BY public.dish_in_menu_history.id;


--
-- Name: food_dish_version_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.food_dish_version_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: dish_version; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dish_version (
    id bigint DEFAULT nextval('public.food_dish_version_id_seq'::regclass) NOT NULL,
    dish_id bigint,
    kcalories numeric,
    weight numeric,
    weight_description character(100),
    image_id character varying(50),
    is_active boolean DEFAULT true NOT NULL,
    last_upd_date timestamp without time zone,
    version_id bigint,
    version_from timestamp without time zone,
    version_to timestamp without time zone,
    base_price_rub numeric(15,2),
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_by bigint,
    cafe_category_link_id bigint DEFAULT 0 NOT NULL,
    food_dish_name character varying(100),
    food_dish_index integer,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE dish_version; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.dish_version IS 'Таблица с историческими версиями блюд';


--
-- Name: COLUMN dish_version.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.id IS 'Идентификатор версии';


--
-- Name: COLUMN dish_version.dish_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.dish_id IS 'Идентификатор блюда';


--
-- Name: COLUMN dish_version.kcalories; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.kcalories IS 'Килокалории';


--
-- Name: COLUMN dish_version.weight; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.weight IS 'Вес, грамм';


--
-- Name: COLUMN dish_version.weight_description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.weight_description IS 'Описание веса';


--
-- Name: COLUMN dish_version.image_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.image_id IS 'Идентификатор изображения версии блюда';


--
-- Name: COLUMN dish_version.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.is_active IS 'Флаг активности блюда';


--
-- Name: COLUMN dish_version.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN dish_version.version_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.version_id IS 'Новер версии ';


--
-- Name: COLUMN dish_version.version_from; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.version_from IS 'Дата начала действия версии';


--
-- Name: COLUMN dish_version.version_to; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.version_to IS 'Дата окончания действия версии';


--
-- Name: COLUMN dish_version.base_price_rub; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.base_price_rub IS 'Базовая цена блюда, в рублях';


--
-- Name: COLUMN dish_version.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.create_date IS 'Дата создания записи';


--
-- Name: COLUMN dish_version.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN dish_version.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.last_upd_by IS 'Идентификатор пользователя, последнего изменившего запись';


--
-- Name: COLUMN dish_version.food_dish_index; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.dish_version.food_dish_index IS 'Порядок вывода блюда в рамках категории';


--
-- Name: food_category_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.food_category_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: food_category_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.food_category_id_seq OWNED BY public.dish_category.id;


--
-- Name: food_dish_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.food_dish_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: food_dish_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.food_dish_id_seq OWNED BY public.dish.id;


--
-- Name: images_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.images_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: images; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.images (
    id bigint DEFAULT nextval('public.images_id_seq'::regclass) NOT NULL,
    object_id bigint,
    object_type_id integer,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL,
    hash character varying(50)
);


--
-- Name: TABLE images; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.images IS 'Таблица с ссылками на изображения';


--
-- Name: COLUMN images.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.id IS 'Идентификатор записи';


--
-- Name: COLUMN images.object_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.object_id IS 'Идентификатор объекта, к которому относится изображение';


--
-- Name: COLUMN images.object_type_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.object_type_id IS 'Тип объекта, к которому относится изображение';


--
-- Name: COLUMN images.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.create_date IS 'Дата создания записи';


--
-- Name: COLUMN images.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN images.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN images.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.images.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: kitchen_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.kitchen_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: kitchen; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.kitchen (
    id bigint DEFAULT nextval('public.kitchen_id_seq'::regclass) NOT NULL,
    kitchen_name character varying(50),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE kitchen; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.kitchen IS 'Список национальных кухонь';


--
-- Name: COLUMN kitchen.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.kitchen.id IS 'Идентификатор национальной кухни';


--
-- Name: COLUMN kitchen.kitchen_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.kitchen.kitchen_name IS 'Наименование национальной кухни';


--
-- Name: log_message; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.log_message (
    id bigint NOT NULL,
    severity character varying(32),
    msg_code character varying(64),
    msg_text text,
    source_package character varying(128),
    msg_datetime timestamp(6) without time zone
);


--
-- Name: COLUMN log_message.severity; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.log_message.severity IS 'Серьёзность сообщения:
- Блокирующая (Blocker)
- Критическая (Critical)
- Значительная (Major)
- Незначительная (Minor) 
- Тривиальная (Trivial)';


--
-- Name: log_message_codes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.log_message_codes (
    id bigint NOT NULL,
    message_code character varying(64),
    code_descr character varying(1024)
);


--
-- Name: log_message_codes_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.log_message_codes_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: log_message_codes_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.log_message_codes_id_seq OWNED BY public.log_message_codes.id;


--
-- Name: log_message_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.log_message_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: log_message_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.log_message_id_seq OWNED BY public.log_message.id;


--
-- Name: notification_channel_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.notification_channel_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: notification_channel; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.notification_channel (
    id bigint DEFAULT nextval('public.notification_channel_id_seq'::regclass) NOT NULL,
    notification_channel_code character varying(30),
    notification_channel_name character varying(50),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE notification_channel; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.notification_channel IS 'Таблица каналов уведомлений (email, sms, phone и т.д.)';


--
-- Name: COLUMN notification_channel.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notification_channel.id IS 'Идентификатор типа уведомлений';


--
-- Name: COLUMN notification_channel.notification_channel_code; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notification_channel.notification_channel_code IS 'Код типа канала уведомления';


--
-- Name: COLUMN notification_channel.notification_channel_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notification_channel.notification_channel_name IS 'Наименование канала уведомления';


--
-- Name: notification_type; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.notification_type (
    id bigint NOT NULL,
    notification_type_code character varying(50),
    notification_type_name character varying(50),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE notification_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.notification_type IS 'Таблица типов уведомлений (Регистрация пользователя, принятие заказа, отмена заказа)';


--
-- Name: COLUMN notification_type.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notification_type.id IS 'Идентификатор типа уведомления';


--
-- Name: COLUMN notification_type.notification_type_code; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notification_type.notification_type_code IS 'Наименование кода типа уведомлений';


--
-- Name: COLUMN notification_type.notification_type_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notification_type.notification_type_name IS 'Наименование типа уведомления';


--
-- Name: notification_type_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.notification_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: notification_type_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.notification_type_id_seq OWNED BY public.notification_type.id;


--
-- Name: notifications_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.notifications_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: notifications; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.notifications (
    id bigint DEFAULT nextval('public.notifications_id_seq'::regclass) NOT NULL,
    order_id bigint,
    user_id bigint,
    cafe_id bigint,
    notification_channel_id bigint,
    notification_type_id bigint,
    send_contact character varying(256),
    send_date timestamp(6) without time zone,
    send_status character varying(1),
    error_message character varying(256),
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE notifications; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.notifications IS 'Таблица учета уведомлений';


--
-- Name: COLUMN notifications.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.id IS 'Идентификатор элемента';


--
-- Name: COLUMN notifications.order_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.order_id IS 'Идентификато заказа, в рамках которого заказали данное блюдо.';


--
-- Name: COLUMN notifications.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.user_id IS 'Идентификатор пользователя, которому был отправлен заказ';


--
-- Name: COLUMN notifications.cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.cafe_id IS 'Идентификатор кафе, которому было отправлено уведомление';


--
-- Name: COLUMN notifications.notification_channel_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.notification_channel_id IS 'Идентификатор канала уведомления';


--
-- Name: COLUMN notifications.notification_type_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.notification_type_id IS 'Идентификатор типа уведомления';


--
-- Name: COLUMN notifications.send_contact; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.send_contact IS 'Контакт, на который было послано уведомление';


--
-- Name: COLUMN notifications.send_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.send_date IS 'Дата отправки уведомления';


--
-- Name: COLUMN notifications.send_status; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.send_status IS 'Статус отправки уведомления (S - success, E - error)';


--
-- Name: COLUMN notifications.error_message; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.notifications.error_message IS 'В случае ошибки - текст ошибки отправки уведомления';


--
-- Name: object_type_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.object_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: object_type; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.object_type (
    id integer DEFAULT nextval('public.object_type_id_seq'::regclass) NOT NULL,
    description character varying(256),
    guid uuid DEFAULT public.uuid_generate_v4(),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE object_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.object_type IS 'Таблица, содержащая определения типов объектов, привязываемых к тегам.';


--
-- Name: COLUMN object_type.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.object_type.id IS 'Идентификатор записи';


--
-- Name: COLUMN object_type.description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.object_type.description IS 'Наименование типа объекта';


--
-- Name: order_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.order_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: order; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."order" (
    id bigint DEFAULT nextval('public.order_id_seq'::regclass) NOT NULL,
    user_id bigint,
    order_create_date timestamp(6) without time zone,
    order_delivery_address bigint,
    order_phone_number character varying(50),
    odd_money_comment character varying(100),
    comment character varying,
    company_order_id bigint,
    order_item_count integer,
    total_price numeric(15,2) DEFAULT 0,
    deliver_datetime timestamp(6) without time zone,
    cafe_id bigint,
    created_by bigint DEFAULT 0,
    create_date timestamp without time zone DEFAULT now(),
    last_upd_by bigint,
    last_upd_date timestamp without time zone,
    is_deleted boolean DEFAULT false NOT NULL,
    state bigint,
    order_info_id bigint,
    banket_id bigint
);


--
-- Name: TABLE "order"; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public."order" IS 'Таблица с заказами пользователей';


--
-- Name: COLUMN "order".id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".id IS 'Идентификатор заказа';


--
-- Name: COLUMN "order".user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".user_id IS 'Пользователь, создавший заказ';


--
-- Name: COLUMN "order".order_create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".order_create_date IS 'Дата создания заказа';


--
-- Name: COLUMN "order".order_delivery_address; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".order_delivery_address IS 'Идентификатор адреса доставки заказа';


--
-- Name: COLUMN "order".order_phone_number; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".order_phone_number IS 'Телефонный номер пользователя для связи';


--
-- Name: COLUMN "order".odd_money_comment; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".odd_money_comment IS 'Нужна сдача с указанной в данном поле суммы рублей';


--
-- Name: COLUMN "order".company_order_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".company_order_id IS 'Идентификатор номера заказа на компанию, если заказ шел на компанию.';


--
-- Name: COLUMN "order".order_item_count; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".order_item_count IS 'Подсчитанное количество элементов в заказе';


--
-- Name: COLUMN "order".total_price; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".total_price IS 'Общая сумма заказ, руб';


--
-- Name: COLUMN "order".deliver_datetime; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".deliver_datetime IS 'Желательная дата и время доставки';


--
-- Name: COLUMN "order".cafe_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."order".cafe_id IS 'Идентификатор кафе';


--
-- Name: order_info; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.order_info (
    id bigint NOT NULL,
    delivery_summ numeric(15,2),
    discount_summ numeric(15,2),
    total_summ numeric(15,2),
    payment_type bigint,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    order_email character varying(256),
    order_phone text,
    order_address text
);


--
-- Name: COLUMN order_info.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_info.id IS 'Идентификатор записи';


--
-- Name: COLUMN order_info.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_info.create_date IS 'Дата создания записи';


--
-- Name: COLUMN order_info.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_info.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN order_info.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_info.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN order_info.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_info.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: order_info_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.order_info_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: order_info_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.order_info_id_seq OWNED BY public.order_info.id;


--
-- Name: order_item_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.order_item_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: order_item; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.order_item (
    id bigint DEFAULT nextval('public.order_item_id_seq'::regclass) NOT NULL,
    dish_id bigint,
    food_dish_name character varying(100),
    food_dish_kcalories numeric,
    food_dish_weight numeric,
    food_dish_image_id bigint,
    dish_count integer,
    dish_base_price numeric(15,2),
    dish_discount_prc integer,
    total_price numeric(15,2) DEFAULT 0,
    order_id bigint,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL,
    comment character varying
);


--
-- Name: TABLE order_item; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.order_item IS 'Таблица элементов заказа';


--
-- Name: COLUMN order_item.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.id IS 'Идентификатор элемента заказа';


--
-- Name: COLUMN order_item.dish_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.dish_id IS 'Идентификатор заказнного блюда';


--
-- Name: COLUMN order_item.food_dish_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.food_dish_name IS 'Сохраненное название заказанного блюад';


--
-- Name: COLUMN order_item.food_dish_kcalories; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.food_dish_kcalories IS 'Сохраненное значение размера Ккал';


--
-- Name: COLUMN order_item.food_dish_weight; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.food_dish_weight IS 'Сохраненное значение веса';


--
-- Name: COLUMN order_item.food_dish_image_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.food_dish_image_id IS 'Сохраненное значение картинки блюда';


--
-- Name: COLUMN order_item.dish_count; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.dish_count IS 'Количество заказанного блюда';


--
-- Name: COLUMN order_item.dish_base_price; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.dish_base_price IS 'Сохраненное значение базовой цены заказанного блюда';


--
-- Name: COLUMN order_item.dish_discount_prc; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.dish_discount_prc IS 'Размер скидки, в процентах';


--
-- Name: COLUMN order_item.total_price; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.total_price IS 'Подсчитанная сумма стоимости данного элемента, в рублях.';


--
-- Name: COLUMN order_item.order_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.order_id IS 'Идентификато заказа, в рамках которого заказали данное блюдо.';


--
-- Name: COLUMN order_item.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.create_date IS 'Дата создания заказа';


--
-- Name: COLUMN order_item.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN order_item.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN order_item.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.last_upd_by IS 'Пользователь, последний изменивший запись.';


--
-- Name: COLUMN order_item.comment; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_item.comment IS 'Комментарий пользователя к блюду заказа';


--
-- Name: order_status; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.order_status (
    status_name character varying(100),
    status_code character varying(50) NOT NULL,
    id bigint NOT NULL,
    guid uuid DEFAULT public.uuid_generate_v4(),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE order_status; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.order_status IS 'Таблица-справочник статусов заказа';


--
-- Name: COLUMN order_status.status_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_status.status_name IS 'Статус заказа';


--
-- Name: COLUMN order_status.status_code; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_status.status_code IS 'Буквенный сокращенный идентификатор статуса заказа
(NEW, ACCEPTED и т.д.)';


--
-- Name: order_status_history_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.order_status_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: order_status_history; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.order_status_history (
    id bigint DEFAULT nextval('public.order_status_history_id_seq'::regclass) NOT NULL,
    order_id bigint,
    order_status integer,
    created_by bigint DEFAULT 0,
    create_date timestamp without time zone DEFAULT now(),
    last_upd_by bigint,
    last_upd_date timestamp without time zone,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE order_status_history; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.order_status_history IS 'Таблица изменения статусов заказов';


--
-- Name: COLUMN order_status_history.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_status_history.id IS 'Идентификатор статуса доставки';


--
-- Name: COLUMN order_status_history.order_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_status_history.order_id IS 'Наименование статуса доставки.';


--
-- Name: COLUMN order_status_history.order_status; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.order_status_history.order_status IS 'Статус заказа';


--
-- Name: order_status_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.order_status_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: order_status_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.order_status_id_seq OWNED BY public.order_status.id;


--
-- Name: qrtz_blob_triggers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_blob_triggers (
    sched_name character varying(120) NOT NULL,
    trigger_name character varying(150) NOT NULL,
    trigger_group character varying(150) NOT NULL,
    blob_data bytea
);


--
-- Name: qrtz_calendars; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_calendars (
    sched_name character varying(120) NOT NULL,
    calendar_name character varying(200) NOT NULL,
    calendar bytea NOT NULL
);


--
-- Name: qrtz_cron_triggers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_cron_triggers (
    sched_name character varying(120) NOT NULL,
    trigger_name character varying(150) NOT NULL,
    trigger_group character varying(150) NOT NULL,
    cron_expression character varying(250) NOT NULL,
    time_zone_id character varying(80)
);


--
-- Name: qrtz_fired_triggers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_fired_triggers (
    sched_name character varying(120) NOT NULL,
    entry_id character varying(140) NOT NULL,
    trigger_name character varying(150) NOT NULL,
    trigger_group character varying(150) NOT NULL,
    instance_name character varying(200) NOT NULL,
    fired_time bigint NOT NULL,
    sched_time bigint NOT NULL,
    priority integer NOT NULL,
    state character varying(16) NOT NULL,
    job_name character varying(200),
    job_group character varying(200),
    is_nonconcurrent boolean NOT NULL,
    requests_recovery boolean
);


--
-- Name: qrtz_job_details; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_job_details (
    sched_name character varying(120) NOT NULL,
    job_name character varying(200) NOT NULL,
    job_group character varying(200) NOT NULL,
    description character varying(250),
    job_class_name character varying(250) NOT NULL,
    is_durable boolean NOT NULL,
    is_nonconcurrent boolean NOT NULL,
    is_update_data boolean NOT NULL,
    requests_recovery boolean NOT NULL,
    job_data bytea
);


--
-- Name: qrtz_locks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_locks (
    sched_name character varying(120) NOT NULL,
    lock_name character varying(40) NOT NULL
);


--
-- Name: qrtz_paused_trigger_grps; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_paused_trigger_grps (
    sched_name character varying(120) NOT NULL,
    trigger_group character varying(150) NOT NULL
);


--
-- Name: qrtz_scheduler_state; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_scheduler_state (
    sched_name character varying(120) NOT NULL,
    instance_name character varying(200) NOT NULL,
    last_checkin_time bigint NOT NULL,
    checkin_interval bigint NOT NULL
);


--
-- Name: qrtz_simple_triggers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_simple_triggers (
    sched_name character varying(120) NOT NULL,
    trigger_name character varying(150) NOT NULL,
    trigger_group character varying(150) NOT NULL,
    repeat_count bigint NOT NULL,
    repeat_interval bigint NOT NULL,
    times_triggered bigint NOT NULL
);


--
-- Name: qrtz_simprop_triggers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_simprop_triggers (
    sched_name character varying(120) NOT NULL,
    trigger_name character varying(150) NOT NULL,
    trigger_group character varying(150) NOT NULL,
    str_prop_1 character varying(512),
    str_prop_2 character varying(512),
    str_prop_3 character varying(512),
    int_prop_1 integer,
    int_prop_2 integer,
    long_prop_1 bigint,
    long_prop_2 bigint,
    dec_prop_1 numeric,
    dec_prop_2 numeric,
    bool_prop_1 boolean,
    bool_prop_2 boolean,
    time_zone_id character varying(80)
);


--
-- Name: qrtz_triggers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.qrtz_triggers (
    sched_name character varying(120) NOT NULL,
    trigger_name character varying(150) NOT NULL,
    trigger_group character varying(150) NOT NULL,
    job_name character varying(200) NOT NULL,
    job_group character varying(200) NOT NULL,
    description character varying(250),
    next_fire_time bigint,
    prev_fire_time bigint,
    priority integer,
    trigger_state character varying(16) NOT NULL,
    trigger_type character varying(8) NOT NULL,
    start_time bigint NOT NULL,
    end_time bigint,
    calendar_name character varying(200),
    misfire_instr smallint,
    job_data bytea
);


--
-- Name: rating_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.rating_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: rating; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.rating (
    id bigint DEFAULT nextval('public.rating_id_seq'::regclass) NOT NULL,
    object_type_id integer,
    object_id bigint,
    user_id bigint,
    rating_value integer,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_by bigint,
    last_upd_date timestamp(6) without time zone,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE rating; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.rating IS 'Рейтинг кафе и блюд';


--
-- Name: COLUMN rating.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.id IS 'Идентификатор записи';


--
-- Name: COLUMN rating.object_type_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.object_type_id IS 'Тип объекта (из таблицы object_type)';


--
-- Name: COLUMN rating.object_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.object_id IS 'Идентификатор объекта';


--
-- Name: COLUMN rating.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.user_id IS 'Идентификатор пользователя, который проставил рейтинг';


--
-- Name: COLUMN rating.rating_value; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.rating_value IS 'Значение рейтинга (1-5)';


--
-- Name: COLUMN rating.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.create_date IS 'Дата создания записи';


--
-- Name: COLUMN rating.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.rating.created_by IS 'Пользователь, создавший запись.';


--
-- Name: referral_coef_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.referral_coef_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: referral_coef; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.referral_coef (
    id bigint DEFAULT nextval('public.referral_coef_id_seq'::regclass) NOT NULL,
    ref_level integer DEFAULT 0,
    coef numeric(15,4) DEFAULT 0
);


--
-- Name: refresh_token_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.refresh_token_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: refresh_token; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.refresh_token (
    id bigint DEFAULT nextval('public.refresh_token_id_seq'::regclass) NOT NULL,
    subject character varying(50) NOT NULL,
    issued_utc date,
    expired_utc date,
    protected_ticket character varying NOT NULL,
    token character varying NOT NULL,
    client_id character varying(100)
);


--
-- Name: reply_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.reply_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: reply; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.reply (
    id bigint DEFAULT nextval('public.reply_id_seq'::regclass) NOT NULL,
    user_id bigint,
    user_name character varying(50),
    object_type character(4),
    reply_text character varying(512),
    reply_rating smallint,
    object_id integer,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE reply; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.reply IS 'Таблица отзывов';


--
-- Name: COLUMN reply.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.id IS 'Идентификатор отзыва';


--
-- Name: COLUMN reply.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.user_id IS 'Идентификатор пользователя, оставившего отзыв';


--
-- Name: COLUMN reply.user_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.user_name IS 'Имя пользователя, которое будет оставлено под отзывом';


--
-- Name: COLUMN reply.object_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.object_type IS 'Тип объекта, для которого оставлен комментарий. Возможные значения:
CAFE
DISH';


--
-- Name: COLUMN reply.reply_text; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.reply_text IS 'Текст отзыва';


--
-- Name: COLUMN reply.reply_rating; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.reply_rating IS 'Рейтинг, поставленный пользователем в рамках данного отзыва. Пятибальная шкала, от 1 до 5 включительно. 1 - отрицательный отзыв, 5 положительный.';


--
-- Name: COLUMN reply.object_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.object_id IS 'Идентификатор объекта, для которого оставлен отзыв. Соответствует cafe.id или food_dish.id';


--
-- Name: COLUMN reply.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.create_date IS 'Дата создания записи';


--
-- Name: COLUMN reply.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.created_by IS 'Пользователь, создавший запись.';


--
-- Name: COLUMN reply.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.last_upd_date IS 'Дата последнего обновления записи';


--
-- Name: COLUMN reply.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reply.last_upd_by IS 'Пользователь, последний обновивший запись.';


--
-- Name: reports_extensions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.reports_extensions (
    id bigint NOT NULL,
    ext character varying(2048),
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE reports_extensions; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.reports_extensions IS 'Таблица расширений отчетов';


--
-- Name: COLUMN reports_extensions.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reports_extensions.create_date IS 'Дата создания записи';


--
-- Name: COLUMN reports_extensions.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reports_extensions.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN reports_extensions.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reports_extensions.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN reports_extensions.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reports_extensions.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN reports_extensions.is_deleted; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.reports_extensions.is_deleted IS 'Флаг логического удаления';


--
-- Name: reports_extensions_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.reports_extensions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: reports_extensions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.reports_extensions_id_seq OWNED BY public.reports_extensions.id;


--
-- Name: role_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.role_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: role; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.role (
    id bigint DEFAULT nextval('public.role_id_seq'::regclass) NOT NULL,
    role_name character varying(256) DEFAULT ''::character varying NOT NULL,
    role_description character varying(300),
    guid uuid DEFAULT public.uuid_generate_v4(),
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE role; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.role IS 'Таблица с ролями пользователей';


--
-- Name: COLUMN role.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.role.id IS 'Идентификатор роли';


--
-- Name: COLUMN role.role_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.role.role_name IS 'Имя роли';


--
-- Name: COLUMN role.role_description; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.role.role_description IS 'Описание роли';


--
-- Name: scheduled_tasks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.scheduled_tasks (
    id bigint NOT NULL,
    execution_time timestamp(6) without time zone,
    company_order_id bigint,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    cafe_id bigint,
    order_id bigint,
    is_repeatable bigint,
    banket_id bigint,
    type integer DEFAULT 0 NOT NULL
);


--
-- Name: scheduled_tasks_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.scheduled_tasks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: scheduled_tasks_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.scheduled_tasks_id_seq OWNED BY public.scheduled_tasks.id;


--
-- Name: sms_code; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sms_code (
    id bigint NOT NULL,
    user_id bigint NOT NULL,
    phone character varying(20) NOT NULL,
    code character varying(20) NOT NULL,
    creation_time timestamp without time zone DEFAULT now() NOT NULL,
    valid_time timestamp without time zone NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE sms_code; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.sms_code IS 'Коды подтверждений, отправляемые пользователям по СМС';


--
-- Name: COLUMN sms_code.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.id IS 'Идентификатор записи';


--
-- Name: COLUMN sms_code.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.user_id IS 'Идентификатор пользователя, которому отправлен код';


--
-- Name: COLUMN sms_code.phone; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.phone IS 'Телефон, на который отправлено СМС с кодом';


--
-- Name: COLUMN sms_code.code; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.code IS 'Текст кода';


--
-- Name: COLUMN sms_code.creation_time; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.creation_time IS 'Время генерации кода';


--
-- Name: COLUMN sms_code.valid_time; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.valid_time IS 'Время, до которого действует код';


--
-- Name: COLUMN sms_code.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.sms_code.is_active IS 'Текущее состояние кода - действует или нет';


--
-- Name: sms_code_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.sms_code_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: sms_code_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.sms_code_id_seq OWNED BY public.sms_code.id;


--
-- Name: tag_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.tag_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: SEQUENCE tag_id_seq; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON SEQUENCE public.tag_id_seq IS 'Последовательность для id тегов';


--
-- Name: tag_object_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.tag_object_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: SEQUENCE tag_object_link_id_seq; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON SEQUENCE public.tag_object_link_id_seq IS 'Последовательность для идентификаторов tag_object_link';


--
-- Name: tag_object_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tag_object_link (
    id bigint DEFAULT nextval('public.tag_object_link_id_seq'::regclass) NOT NULL,
    tag_id bigint,
    object_id integer,
    object_type_id integer,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE tag_object_link; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.tag_object_link IS 'Таблица связки объектов с тегом';


--
-- Name: COLUMN tag_object_link.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.id IS 'Идентификатор записи';


--
-- Name: COLUMN tag_object_link.tag_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.tag_id IS 'Идентификатор связанного тега';


--
-- Name: COLUMN tag_object_link.object_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.object_id IS 'Идентификатор объекта, который привязан к тегу';


--
-- Name: COLUMN tag_object_link.object_type_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.object_type_id IS 'Идентификатор типа объекта';


--
-- Name: COLUMN tag_object_link.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.create_date IS 'Дата создания записи';


--
-- Name: COLUMN tag_object_link.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.created_by IS 'Автор создания записи';


--
-- Name: COLUMN tag_object_link.last_upd; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.last_upd IS 'Дата последнего изменения записи';


--
-- Name: COLUMN tag_object_link.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tag_object_link.last_upd_by IS 'Идентификатор пользователя, который последний изменил запись';


--
-- Name: tags; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.tags (
    name character varying(256),
    parent_id bigint,
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd timestamp(6) without time zone,
    last_upd_by bigint,
    id bigint DEFAULT nextval('public.tag_id_seq'::regclass) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE tags; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.tags IS 'Таблица тегов';


--
-- Name: COLUMN tags.name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.name IS 'Имя тега';


--
-- Name: COLUMN tags.parent_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.parent_id IS 'Ссылка на родительский тег, если таковой имеется';


--
-- Name: COLUMN tags.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.create_date IS 'Дата и время создания';


--
-- Name: COLUMN tags.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.created_by IS 'Идентификатор создателя';


--
-- Name: COLUMN tags.last_upd; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.last_upd IS 'Дата последнего обновления';


--
-- Name: COLUMN tags.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.last_upd_by IS 'Идентификатор пользователя, который последний обновил запись';


--
-- Name: COLUMN tags.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.id IS 'Идентификатор записи';


--
-- Name: COLUMN tags.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.tags.is_active IS 'Флаг активности тега';


--
-- Name: user_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."user" (
    id bigint DEFAULT nextval('public.user_id_seq'::regclass) NOT NULL,
    user_access_failed_count integer DEFAULT 0 NOT NULL,
    user_email character varying(256),
    user_email_confirmed boolean DEFAULT false NOT NULL,
    user_lockout_enabled boolean DEFAULT false NOT NULL,
    user_lockout_enddate_utc timestamp without time zone,
    user_password text,
    user_phone text,
    user_phone_confirmed boolean DEFAULT false NOT NULL,
    user_security_stamp text,
    user_twofactor_enabled boolean DEFAULT false NOT NULL,
    user_name character varying(256) DEFAULT ''::character varying,
    user_fullname text,
    create_date timestamp without time zone DEFAULT now(),
    default_address_id bigint,
    user_device_uuid character varying(256),
    user_display_name character varying(50),
    created_by bigint,
    last_upd_date timestamp without time zone,
    last_upd_by bigint,
    user_first_name character varying(50),
    user_surname character varying(50),
    is_deleted boolean DEFAULT false NOT NULL,
    personal_points numeric(15,4) DEFAULT 0 NOT NULL,
    referral_points numeric(15,4) DEFAULT 0 NOT NULL,
    percent_of_order numeric(3,2) DEFAULT 0.05 NOT NULL,
    user_referral_link character varying(10),
    sms_notify boolean DEFAULT false
);


--
-- Name: COLUMN "user".id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".id IS 'Идентификатор пользователя';


--
-- Name: COLUMN "user".user_access_failed_count; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_access_failed_count IS 'Количество неуспешных попыток входа';


--
-- Name: COLUMN "user".user_email; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_email IS 'Email пользователя
';


--
-- Name: COLUMN "user".user_email_confirmed; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_email_confirmed IS 'Подтвержден email или нет';


--
-- Name: COLUMN "user".user_lockout_enabled; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_lockout_enabled IS 'Признак блокировки пользователя';


--
-- Name: COLUMN "user".user_password; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_password IS 'Пароль пользователя';


--
-- Name: COLUMN "user".user_phone; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_phone IS 'Телфон пользователя';


--
-- Name: COLUMN "user".user_phone_confirmed; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_phone_confirmed IS 'Подтвежден телефон или нет';


--
-- Name: COLUMN "user".user_twofactor_enabled; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_twofactor_enabled IS 'Признак двухфакторной аутентификации';


--
-- Name: COLUMN "user".user_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_name IS 'Имя пользователя';


--
-- Name: COLUMN "user".user_fullname; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_fullname IS 'Полное имя пользователя';


--
-- Name: COLUMN "user".create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".create_date IS 'Дата создания пользователя';


--
-- Name: COLUMN "user".default_address_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".default_address_id IS 'Адрес пользователя для доставки по умолчанию';


--
-- Name: COLUMN "user".user_device_uuid; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_device_uuid IS 'Идентификатор устройства пользователя';


--
-- Name: COLUMN "user".created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".created_by IS 'Пользователь, создавший запись, если Null - пользователь создал себя сам';


--
-- Name: COLUMN "user".last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN "user".last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN "user".user_first_name; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_first_name IS 'Имя пользователя';


--
-- Name: COLUMN "user".user_surname; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".user_surname IS 'Фамилия пользователя';


--
-- Name: COLUMN "user".personal_points; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".personal_points IS 'Лично заработанные баллы';


--
-- Name: COLUMN "user".referral_points; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".referral_points IS 'Баллы от реферальной программы';


--
-- Name: COLUMN "user".percent_of_order; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".percent_of_order IS 'Процент от заказа';


--
-- Name: COLUMN "user".sms_notify; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public."user".sms_notify IS 'Включает/отключает отправку СМС-оповещений по заказам для пользователя';


--
-- Name: user_claim_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_claim_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user_company_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_company_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user_company_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_company_link (
    id bigint DEFAULT nextval('public.user_company_link_id_seq'::regclass) NOT NULL,
    user_id bigint,
    company_id bigint,
    is_active boolean DEFAULT true NOT NULL,
    link_start_date date DEFAULT (date_trunc('month'::text, now()))::date,
    link_end_date date DEFAULT ((date_trunc('century'::text, (now() + '100 years'::interval)) - '00:00:01'::interval))::date,
    user_type character varying(10),
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp without time zone,
    last_upd_by integer,
    role_id bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    default_address_id bigint
);


--
-- Name: TABLE user_company_link; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.user_company_link IS 'Таблица привязки пользователя к компании. Позволяет делать данному пользователю заказ на привязанную компанию.';


--
-- Name: COLUMN user_company_link.id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.id IS 'Идентификатор привязки пользователя к компании';


--
-- Name: COLUMN user_company_link.user_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.user_id IS 'Идентификатор пользователя';


--
-- Name: COLUMN user_company_link.company_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.company_id IS 'Идентификатор компании';


--
-- Name: COLUMN user_company_link.is_active; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.is_active IS 'Признак активности данной записи';


--
-- Name: COLUMN user_company_link.link_start_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.link_start_date IS 'Дата начала действия привязки';


--
-- Name: COLUMN user_company_link.link_end_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.link_end_date IS 'Дата окончания действия привязки';


--
-- Name: COLUMN user_company_link.user_type; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.user_type IS 'Тип пользователя в данной организации. Возможные варианты:
USER
MANAGER
REPORT

USER - обычный пользователь, функционал создания личного заказа в рамках общего заказа организации;
MANAGER - максимальные права в рамках данной компании, управляет заказами компании, может удалять/изменять заказы пользователей компании, просматриват отчеты;
REPORT - функционал создания личного заказа в рамках общего заказа организации, кроме этого может просматривать отчеты по всем заказам организации. Обычно директор или бухг';


--
-- Name: COLUMN user_company_link.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.create_date IS 'Дата создания записи';


--
-- Name: COLUMN user_company_link.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN user_company_link.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.last_upd_date IS 'Дата последнего обновления записи';


--
-- Name: COLUMN user_company_link.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.last_upd_by IS 'Пользователь, последний обновивший запись.';


--
-- Name: COLUMN user_company_link.role_id; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.user_company_link.role_id IS 'Идентификатор роли пользователя';


--
-- Name: user_external_login; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_external_login (
    login_provider character varying(128) DEFAULT ''::character varying NOT NULL,
    provider_key character varying(128) DEFAULT ''::character varying NOT NULL,
    user_id bigint DEFAULT 0 NOT NULL
);


--
-- Name: user_referral_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_referral_link (
    id bigint NOT NULL,
    parent_id bigint,
    referral_id bigint,
    is_active boolean DEFAULT true NOT NULL,
    ref_level integer DEFAULT 0,
    create_date timestamp(6) without time zone DEFAULT now(),
    is_deleted boolean DEFAULT false NOT NULL,
    earned_points numeric(15,4) DEFAULT 0,
    path_index integer,
    num_mapping text,
    root_id bigint
);


--
-- Name: user_referral_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_referral_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user_referral_link_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.user_referral_link_id_seq OWNED BY public.user_referral_link.id;


--
-- Name: user_role_link_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.user_role_link_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: user_role_link; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_role_link (
    id bigint DEFAULT nextval('public.user_role_link_id_seq'::regclass) NOT NULL,
    user_id bigint DEFAULT 0 NOT NULL,
    role_id bigint DEFAULT 0 NOT NULL,
    is_deleted boolean DEFAULT false NOT NULL
);


--
-- Name: v_today_cafe_menu; Type: MATERIALIZED VIEW; Schema: public; Owner: -
--

CREATE MATERIALIZED VIEW public.v_today_cafe_menu AS
 WITH t_dish AS (
         SELECT d1.id,
            d1.dish_id,
            d1.s_type,
            d1.is_active,
            d1.schedule_begin_date,
            d1.schedule_end_date,
            d1.s_month_day,
            d1.s_week_day,
            d1.food_price_rub,
            d1.one_time_date,
            d1.last_upd_date,
            d1.last_upd_by,
            d1.create_date,
            d1.created_by,
            (now())::date AS schedule_date,
                CASE
                    WHEN (d1.s_type = ANY (ARRAY['D'::bpchar, 'W'::bpchar, 'M'::bpchar, 'd'::bpchar, 'w'::bpchar, 'm'::bpchar])) THEN 1
                    WHEN (d1.s_type = ANY (ARRAY['S'::bpchar, 's'::bpchar])) THEN 2
                    WHEN (d1.s_type = ANY (ARRAY['E'::bpchar, 'e'::bpchar])) THEN 0
                    ELSE NULL::integer
                END AS priority
           FROM ( SELECT dish_in_menu.id,
                    dish_in_menu.dish_id,
                    dish_in_menu.s_type,
                    dish_in_menu.is_active,
                    dish_in_menu.schedule_begin_date,
                    dish_in_menu.schedule_end_date,
                    dish_in_menu.s_month_day,
                    dish_in_menu.s_week_day,
                    dish_in_menu.food_price_rub,
                    dish_in_menu.one_time_date,
                    dish_in_menu.last_upd_date,
                    dish_in_menu.last_upd_by,
                    dish_in_menu.create_date,
                    dish_in_menu.created_by
                   FROM public.dish_in_menu
                  WHERE ((dish_in_menu.s_type = 'D'::bpchar) AND (dish_in_menu.is_active = true) AND (dish_in_menu.schedule_begin_date <= (now())::date) AND (COALESCE(dish_in_menu.schedule_end_date, '2099-12-31'::date) >= (now())::date))
                UNION ALL
                 SELECT dish_in_menu.id,
                    dish_in_menu.dish_id,
                    dish_in_menu.s_type,
                    dish_in_menu.is_active,
                    dish_in_menu.schedule_begin_date,
                    dish_in_menu.schedule_end_date,
                    dish_in_menu.s_month_day,
                    dish_in_menu.s_week_day,
                    dish_in_menu.food_price_rub,
                    dish_in_menu.one_time_date,
                    dish_in_menu.last_upd_date,
                    dish_in_menu.last_upd_by,
                    dish_in_menu.create_date,
                    dish_in_menu.created_by
                   FROM public.dish_in_menu
                  WHERE ((dish_in_menu.s_type = 'W'::bpchar) AND (dish_in_menu.is_active = true) AND (dish_in_menu.schedule_begin_date <= (now())::date) AND (COALESCE(dish_in_menu.schedule_end_date, '2099-12-31'::date) >= (now())::date) AND (to_char(((now())::date)::timestamp with time zone, 'ID'::text) IN ( SELECT regexp_split_to_table((dish_in_menu.s_week_day)::text, ','::text) AS regexp_split_to_table)))
                UNION ALL
                 SELECT dish_in_menu.id,
                    dish_in_menu.dish_id,
                    dish_in_menu.s_type,
                    dish_in_menu.is_active,
                    dish_in_menu.schedule_begin_date,
                    dish_in_menu.schedule_end_date,
                    dish_in_menu.s_month_day,
                    dish_in_menu.s_week_day,
                    dish_in_menu.food_price_rub,
                    dish_in_menu.one_time_date,
                    dish_in_menu.last_upd_date,
                    dish_in_menu.last_upd_by,
                    dish_in_menu.create_date,
                    dish_in_menu.created_by
                   FROM public.dish_in_menu
                  WHERE ((dish_in_menu.s_type = 'M'::bpchar) AND (dish_in_menu.is_active = true) AND (dish_in_menu.schedule_begin_date <= (now())::date) AND (COALESCE(dish_in_menu.schedule_end_date, '2099-12-31'::date) >= (now())::date) AND (to_char(((now())::date)::timestamp with time zone, 'DD'::text) IN ( SELECT regexp_split_to_table((dish_in_menu.s_month_day)::text, ','::text) AS regexp_split_to_table)))
                UNION ALL
                 SELECT dish_in_menu.id,
                    dish_in_menu.dish_id,
                    dish_in_menu.s_type,
                    dish_in_menu.is_active,
                    dish_in_menu.schedule_begin_date,
                    dish_in_menu.schedule_end_date,
                    dish_in_menu.s_month_day,
                    dish_in_menu.s_week_day,
                    dish_in_menu.food_price_rub,
                    dish_in_menu.one_time_date,
                    dish_in_menu.last_upd_date,
                    dish_in_menu.last_upd_by,
                    dish_in_menu.create_date,
                    dish_in_menu.created_by
                   FROM public.dish_in_menu
                  WHERE ((dish_in_menu.s_type = 'S'::bpchar) AND (dish_in_menu.is_active = true) AND (dish_in_menu.one_time_date = (now())::date))) d1
          ORDER BY d1.dish_id
        ), t_dish_except AS (
         SELECT fs.id,
            fs.dish_id,
            fs.s_type,
            fs.is_active,
            fs.schedule_begin_date,
            fs.schedule_end_date,
            fs.s_month_day,
            fs.s_week_day,
            fs.food_price_rub,
            fs.one_time_date,
            fs.last_upd_date,
            fs.last_upd_by,
            fs.create_date,
            fs.created_by,
                CASE
                    WHEN (fs.s_type = ANY (ARRAY['D'::bpchar, 'W'::bpchar, 'M'::bpchar, 'd'::bpchar, 'w'::bpchar, 'm'::bpchar])) THEN 1
                    WHEN (fs.s_type = ANY (ARRAY['S'::bpchar, 's'::bpchar])) THEN 2
                    WHEN (fs.s_type = ANY (ARRAY['E'::bpchar, 'e'::bpchar])) THEN 0
                    ELSE NULL::integer
                END AS priority
           FROM public.dish_in_menu fs
          WHERE ((fs.s_type = 'E'::bpchar) AND (fs.is_active = true) AND (fs.one_time_date = (now())::date))
        ), t_dish_per_date AS (
         SELECT t_dish.dish_id
           FROM t_dish
        EXCEPT
         SELECT t_dish_except.dish_id
           FROM t_dish_except
        ), t_schedule AS (
         SELECT td.id,
            td.dish_id,
            td.s_type,
            td.is_active,
            td.schedule_begin_date,
            td.schedule_end_date,
            td.s_month_day,
            td.s_week_day,
            td.food_price_rub,
            td.one_time_date,
            td.last_upd_date,
            td.last_upd_by,
            td.create_date,
            td.created_by,
            td.schedule_date,
            td.priority
           FROM t_dish td
          WHERE (td.dish_id IN ( SELECT t_dish_per_date.dish_id
                   FROM t_dish_per_date))
        )
 SELECT ts.id AS schedule_id,
    ts.schedule_date,
    c.cafe_name,
    fc.category_name,
    ts.dish_id AS schedule_dish_id,
    d.food_dish_name,
    d.kcalories,
    d.weight,
    d.weight_description,
    COALESCE(ts.food_price_rub, d.base_price_rub) AS schedule_price
   FROM ((((t_schedule ts
     JOIN public.dish d ON (((d.id = ts.dish_id) AND (COALESCE(d.is_active, false) = true) AND (ts.schedule_date >= COALESCE(d.version_from, '2000-01-01'::date)) AND (ts.schedule_date <= COALESCE(d.version_to, '2099-12-31'::date)))))
     JOIN public.cafe_category_link ccl ON ((ccl.id = d.cafe_category_link_id)))
     JOIN public.cafe c ON ((c.id = ccl.cafe_id)))
     JOIN public.dish_category fc ON ((fc.id = ccl.category_id)))
  WHERE ((ts.id = ( SELECT max(tmp.id) AS max
           FROM t_schedule tmp
          WHERE ((tmp.dish_id = ts.dish_id) AND (tmp.s_type = ts.s_type)))) AND (ts.priority = ( SELECT max(t2.priority) AS max
           FROM t_schedule t2
          WHERE (t2.dish_id = ts.dish_id))))
  ORDER BY fc.id
  WITH NO DATA;


--
-- Name: xslt_to_cafe; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.xslt_to_cafe (
    id bigint NOT NULL,
    xslt_id bigint NOT NULL,
    id_cafe bigint NOT NULL,
    is_deleted boolean DEFAULT false,
    create_date timestamp without time zone DEFAULT now(),
    created_by bigint,
    last_upd_date timestamp without time zone,
    last_upd_by bigint
);


--
-- Name: xslt_to_cafe_id_cafe_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.xslt_to_cafe_id_cafe_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: xslt_to_cafe_id_cafe_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.xslt_to_cafe_id_cafe_seq OWNED BY public.xslt_to_cafe.id_cafe;


--
-- Name: xslt_to_cafe_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.xslt_to_cafe_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: xslt_to_cafe_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.xslt_to_cafe_id_seq OWNED BY public.xslt_to_cafe.id;


--
-- Name: xslt_to_cafe_xslt_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.xslt_to_cafe_xslt_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: xslt_to_cafe_xslt_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.xslt_to_cafe_xslt_id_seq OWNED BY public.xslt_to_cafe.xslt_id;


--
-- Name: xslt_transform; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.xslt_transform (
    id bigint NOT NULL,
    cafe_id bigint,
    ext_id bigint,
    xslt text,
    name_template character varying(128),
    description character varying(1024),
    create_date timestamp(6) without time zone DEFAULT now(),
    created_by bigint DEFAULT 0,
    last_upd_date timestamp(6) without time zone,
    last_upd_by bigint,
    is_deleted boolean DEFAULT false NOT NULL,
    is_common boolean DEFAULT false NOT NULL
);


--
-- Name: TABLE xslt_transform; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON TABLE public.xslt_transform IS 'Таблица xslt преобразований';


--
-- Name: COLUMN xslt_transform.create_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.xslt_transform.create_date IS 'Дата создания записи';


--
-- Name: COLUMN xslt_transform.created_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.xslt_transform.created_by IS 'Пользователь, создавший запись';


--
-- Name: COLUMN xslt_transform.last_upd_date; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.xslt_transform.last_upd_date IS 'Дата последнего изменения записи';


--
-- Name: COLUMN xslt_transform.last_upd_by; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.xslt_transform.last_upd_by IS 'Пользователь, последний изменивший запись';


--
-- Name: COLUMN xslt_transform.is_deleted; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.xslt_transform.is_deleted IS 'Флаг логического удаления';


--
-- Name: xslt_transform_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.xslt_transform_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: xslt_transform_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.xslt_transform_id_seq OWNED BY public.xslt_transform.id;


--
-- Name: actions id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.actions ALTER COLUMN id SET DEFAULT nextval('public.actions_id_seq'::regclass);


--
-- Name: address_company_link id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.address_company_link ALTER COLUMN id SET DEFAULT nextval('public.address_company_link_id_seq'::regclass);


--
-- Name: cafe_discount id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_discount ALTER COLUMN id SET DEFAULT nextval('public.cafe_discount_id_seq'::regclass);


--
-- Name: cafe_order_notification id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_order_notification ALTER COLUMN id SET DEFAULT nextval('public.cafe_order_notification_id_seq'::regclass);


--
-- Name: cafe_order_notification cafe_id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_order_notification ALTER COLUMN cafe_id SET DEFAULT nextval('public.cafe_order_notification_id_seq'::regclass);


--
-- Name: cafe_order_notification user_id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_order_notification ALTER COLUMN user_id SET DEFAULT nextval('public.cafe_order_notifications_user_id_seq'::regclass);


--
-- Name: company_order_status_history id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_order_status_history ALTER COLUMN id SET DEFAULT nextval('public.company_order_status_history_id_seq'::regclass);


--
-- Name: cost_of_delivery id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cost_of_delivery ALTER COLUMN id SET DEFAULT nextval('public.cost_of_delivery_id_seq'::regclass);


--
-- Name: dish id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish ALTER COLUMN id SET DEFAULT nextval('public.food_dish_id_seq'::regclass);


--
-- Name: dish_category id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_category ALTER COLUMN id SET DEFAULT nextval('public.food_category_id_seq'::regclass);


--
-- Name: dish_in_menu_history id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_in_menu_history ALTER COLUMN id SET DEFAULT nextval('public.dish_in_menu_history_id_seq'::regclass);


--
-- Name: log_message id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.log_message ALTER COLUMN id SET DEFAULT nextval('public.log_message_id_seq'::regclass);


--
-- Name: log_message_codes id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.log_message_codes ALTER COLUMN id SET DEFAULT nextval('public.log_message_codes_id_seq'::regclass);


--
-- Name: notification_type id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notification_type ALTER COLUMN id SET DEFAULT nextval('public.notification_type_id_seq'::regclass);


--
-- Name: order_info id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_info ALTER COLUMN id SET DEFAULT nextval('public.order_info_id_seq'::regclass);


--
-- Name: order_status id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_status ALTER COLUMN id SET DEFAULT nextval('public.order_status_id_seq'::regclass);


--
-- Name: reports_extensions id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reports_extensions ALTER COLUMN id SET DEFAULT nextval('public.reports_extensions_id_seq'::regclass);


--
-- Name: scheduled_tasks id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.scheduled_tasks ALTER COLUMN id SET DEFAULT nextval('public.scheduled_tasks_id_seq'::regclass);


--
-- Name: sms_code id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sms_code ALTER COLUMN id SET DEFAULT nextval('public.sms_code_id_seq'::regclass);


--
-- Name: user_referral_link id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_referral_link ALTER COLUMN id SET DEFAULT nextval('public.user_referral_link_id_seq'::regclass);


--
-- Name: xslt_to_cafe id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_to_cafe ALTER COLUMN id SET DEFAULT nextval('public.xslt_to_cafe_id_seq'::regclass);


--
-- Name: xslt_to_cafe xslt_id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_to_cafe ALTER COLUMN xslt_id SET DEFAULT nextval('public.xslt_to_cafe_xslt_id_seq'::regclass);


--
-- Name: xslt_to_cafe id_cafe; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_to_cafe ALTER COLUMN id_cafe SET DEFAULT nextval('public.xslt_to_cafe_id_cafe_seq'::regclass);


--
-- Name: xslt_transform id; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_transform ALTER COLUMN id SET DEFAULT nextval('public.xslt_transform_id_seq'::regclass);


--
-- Name: dish_category PK_public.food_category; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_category
    ADD CONSTRAINT "PK_public.food_category" PRIMARY KEY (id);


--
-- Name: dish PK_public.food_dish; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish
    ADD CONSTRAINT "PK_public.food_dish" PRIMARY KEY (id);


--
-- Name: dish_version PK_public.food_dish_version; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_version
    ADD CONSTRAINT "PK_public.food_dish_version" PRIMARY KEY (id);


--
-- Name: log_message PK_public.log_message; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.log_message
    ADD CONSTRAINT "PK_public.log_message" PRIMARY KEY (id);


--
-- Name: log_message_codes PK_public.log_message_codes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.log_message_codes
    ADD CONSTRAINT "PK_public.log_message_codes" PRIMARY KEY (id);


--
-- Name: role PK_public.role; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.role
    ADD CONSTRAINT "PK_public.role" PRIMARY KEY (id);


--
-- Name: sms_code PK_public.sms_code; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sms_code
    ADD CONSTRAINT "PK_public.sms_code" PRIMARY KEY (id);


--
-- Name: user PK_public.user; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT "PK_public.user" PRIMARY KEY (id);


--
-- Name: user_external_login PK_public.user_external_login; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_external_login
    ADD CONSTRAINT "PK_public.user_external_login" PRIMARY KEY (login_provider, provider_key, user_id);


--
-- Name: user_role_link PK_public.user_role_link; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_role_link
    ADD CONSTRAINT "PK_public.user_role_link" PRIMARY KEY (id);


--
-- Name: actions actions_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.actions
    ADD CONSTRAINT actions_pkey PRIMARY KEY (id);


--
-- Name: address_company_link address_company_link_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.address_company_link
    ADD CONSTRAINT address_company_link_pkey PRIMARY KEY (id);


--
-- Name: address address_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.address
    ADD CONSTRAINT address_pkey PRIMARY KEY (id);


--
-- Name: bankets bankets_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankets
    ADD CONSTRAINT bankets_pkey PRIMARY KEY (id);


--
-- Name: cafe_category_link cafe_category_link_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_category_link
    ADD CONSTRAINT cafe_category_link_pkey PRIMARY KEY (id);


--
-- Name: cafe_discount cafe_discount_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_discount
    ADD CONSTRAINT cafe_discount_pkey PRIMARY KEY (id);


--
-- Name: cafe_kitchen_link cafe_kitchen_link_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_kitchen_link
    ADD CONSTRAINT cafe_kitchen_link_pkey PRIMARY KEY (id);


--
-- Name: cafe_managers cafe_managers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_managers
    ADD CONSTRAINT cafe_managers_pkey PRIMARY KEY (id);


--
-- Name: cafe_menu_patterns_dishes cafe_menu_patterns_dishes_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_menu_patterns_dishes
    ADD CONSTRAINT cafe_menu_patterns_dishes_pkey PRIMARY KEY (id);


--
-- Name: cafe_notification_contacts cafe_notification_contacts_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_notification_contacts
    ADD CONSTRAINT cafe_notification_contacts_pkey PRIMARY KEY (id);


--
-- Name: cafe_order_notification cafe_order_notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_order_notification
    ADD CONSTRAINT cafe_order_notifications_pkey PRIMARY KEY (id);


--
-- Name: cafe cafe_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe
    ADD CONSTRAINT cafe_pkey PRIMARY KEY (id);


--
-- Name: cafe_specialization cafe_specialization_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_specialization
    ADD CONSTRAINT cafe_specialization_pkey PRIMARY KEY (id);


--
-- Name: company_curators company_consolidators_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_curators
    ADD CONSTRAINT company_consolidators_pkey PRIMARY KEY (id);


--
-- Name: company_order company_order_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_order
    ADD CONSTRAINT company_order_pkey PRIMARY KEY (id);


--
-- Name: company_order_schedule company_order_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_order_schedule
    ADD CONSTRAINT company_order_schedule_pkey PRIMARY KEY (id);


--
-- Name: company_order_status_history company_order_status_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_order_status_history
    ADD CONSTRAINT company_order_status_pkey PRIMARY KEY (id);


--
-- Name: company company_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_pkey PRIMARY KEY (id);


--
-- Name: cost_of_delivery cost_of_delivery_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cost_of_delivery
    ADD CONSTRAINT cost_of_delivery_pkey PRIMARY KEY (id);


--
-- Name: discount discounts_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.discount
    ADD CONSTRAINT discounts_pkey PRIMARY KEY (id);


--
-- Name: dish_in_menu food_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_in_menu
    ADD CONSTRAINT food_schedule_pkey PRIMARY KEY (id);


--
-- Name: images images_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.images
    ADD CONSTRAINT images_pkey PRIMARY KEY (id);


--
-- Name: kitchen kitchen_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.kitchen
    ADD CONSTRAINT kitchen_pkey PRIMARY KEY (id);


--
-- Name: log_message_codes log_message_codes_message_code_uniq; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.log_message_codes
    ADD CONSTRAINT log_message_codes_message_code_uniq UNIQUE (message_code);


--
-- Name: notification_channel notification_channel_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notification_channel
    ADD CONSTRAINT notification_channel_pkey PRIMARY KEY (id);


--
-- Name: notification_type notification_type_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notification_type
    ADD CONSTRAINT notification_type_pkey PRIMARY KEY (id);


--
-- Name: order_status_history order_delivery_status_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_status_history
    ADD CONSTRAINT order_delivery_status_pkey PRIMARY KEY (id);


--
-- Name: order_info order_info_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_info
    ADD CONSTRAINT order_info_pkey PRIMARY KEY (id);


--
-- Name: order_item order_item_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_item
    ADD CONSTRAINT order_item_pkey PRIMARY KEY (id);


--
-- Name: notifications order_notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT order_notifications_pkey PRIMARY KEY (id);


--
-- Name: order order_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT order_pkey PRIMARY KEY (id);


--
-- Name: order_status order_status_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_status
    ADD CONSTRAINT order_status_pkey PRIMARY KEY (id);


--
-- Name: qrtz_blob_triggers qrtz_blob_triggers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_blob_triggers
    ADD CONSTRAINT qrtz_blob_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_calendars qrtz_calendars_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_calendars
    ADD CONSTRAINT qrtz_calendars_pkey PRIMARY KEY (sched_name, calendar_name);


--
-- Name: qrtz_cron_triggers qrtz_cron_triggers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_cron_triggers
    ADD CONSTRAINT qrtz_cron_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_fired_triggers qrtz_fired_triggers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_fired_triggers
    ADD CONSTRAINT qrtz_fired_triggers_pkey PRIMARY KEY (sched_name, entry_id);


--
-- Name: qrtz_job_details qrtz_job_details_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_job_details
    ADD CONSTRAINT qrtz_job_details_pkey PRIMARY KEY (sched_name, job_name, job_group);


--
-- Name: qrtz_locks qrtz_locks_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_locks
    ADD CONSTRAINT qrtz_locks_pkey PRIMARY KEY (sched_name, lock_name);


--
-- Name: qrtz_paused_trigger_grps qrtz_paused_trigger_grps_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_paused_trigger_grps
    ADD CONSTRAINT qrtz_paused_trigger_grps_pkey PRIMARY KEY (sched_name, trigger_group);


--
-- Name: qrtz_scheduler_state qrtz_scheduler_state_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_scheduler_state
    ADD CONSTRAINT qrtz_scheduler_state_pkey PRIMARY KEY (sched_name, instance_name);


--
-- Name: qrtz_simple_triggers qrtz_simple_triggers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_simple_triggers
    ADD CONSTRAINT qrtz_simple_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_simprop_triggers qrtz_simprop_triggers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_simprop_triggers
    ADD CONSTRAINT qrtz_simprop_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: qrtz_triggers qrtz_triggers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_triggers
    ADD CONSTRAINT qrtz_triggers_pkey PRIMARY KEY (sched_name, trigger_name, trigger_group);


--
-- Name: rating rating_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.rating
    ADD CONSTRAINT rating_pkey PRIMARY KEY (id);


--
-- Name: referral_coef referral_coef_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.referral_coef
    ADD CONSTRAINT referral_coef_pkey PRIMARY KEY (id);


--
-- Name: reply reply_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reply
    ADD CONSTRAINT reply_pkey PRIMARY KEY (id);


--
-- Name: reports_extensions reports_extensions_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.reports_extensions
    ADD CONSTRAINT reports_extensions_pkey PRIMARY KEY (id);


--
-- Name: scheduled_tasks scheduled_tasks_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.scheduled_tasks
    ADD CONSTRAINT scheduled_tasks_pkey PRIMARY KEY (id);


--
-- Name: tag_object_link tag_object_link_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tag_object_link
    ADD CONSTRAINT tag_object_link_pkey PRIMARY KEY (id);


--
-- Name: tags tag_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tag_pkey PRIMARY KEY (id);


--
-- Name: CONSTRAINT tag_pkey ON tags; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON CONSTRAINT tag_pkey ON public.tags IS 'Первичный ключ для тегов';


--
-- Name: user_company_link user_company_link_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_company_link
    ADD CONSTRAINT user_company_link_pkey PRIMARY KEY (id);


--
-- Name: user_referral_link user_referral_link_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_referral_link
    ADD CONSTRAINT user_referral_link_pkey PRIMARY KEY (id);


--
-- Name: xslt_to_cafe xslt_to_cafe_pk; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_to_cafe
    ADD CONSTRAINT xslt_to_cafe_pk PRIMARY KEY (id);


--
-- Name: xslt_transform xslt_transform_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_transform
    ADD CONSTRAINT xslt_transform_pkey PRIMARY KEY (id);


--
-- Name: bankets_id_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX bankets_id_uindex ON public.bankets USING btree (id);


--
-- Name: cafe_category_link_idx1; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX cafe_category_link_idx1 ON public.cafe_category_link USING btree (category_index, id);


--
-- Name: cafe_category_link_idx2; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX cafe_category_link_idx2 ON public.cafe_category_link USING btree (id);


--
-- Name: cafe_category_link_idx3; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX cafe_category_link_idx3 ON public.cafe_category_link USING btree (cafe_id, category_id, category_index);


--
-- Name: cafe_menu_patterns_id_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX cafe_menu_patterns_id_uindex ON public.cafe_menu_patterns USING btree (id);


--
-- Name: cost_of_delivery_for_company_orders_ind; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX cost_of_delivery_for_company_orders_ind ON public.cost_of_delivery USING btree (for_company_orders DESC);


--
-- Name: fki_address_company_link_address_id_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_address_company_link_address_id_fkey ON public.address_company_link USING btree (address_id);


--
-- Name: fki_address_company_link_company_id_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_address_company_link_company_id_fkey ON public.address_company_link USING btree (company_id);


--
-- Name: fki_cafe_discount_cafe_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_cafe_discount_cafe_fkey ON public.cafe_discount USING btree (cafe_id);


--
-- Name: fki_cafe_discount_company_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_cafe_discount_company_fkey ON public.cafe_discount USING btree (company_id);


--
-- Name: fki_company_order_status_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_company_order_status_fkey ON public.company_order USING btree (state);


--
-- Name: fki_log_message_msg_code_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_log_message_msg_code_fkey ON public.log_message USING btree (msg_code);


--
-- Name: fki_order_status_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_order_status_fkey ON public."order" USING btree (state);


--
-- Name: fki_scheduled_tasks_company_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_scheduled_tasks_company_order_id ON public.scheduled_tasks USING btree (company_order_id);


--
-- Name: fki_sms_code_user; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_sms_code_user ON public.sms_code USING btree (user_id DESC);


--
-- Name: fki_tag_object_link_tag_id_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_tag_object_link_tag_id_fkey ON public.tag_object_link USING btree (tag_id);


--
-- Name: fki_tag_parent_id_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_tag_parent_id_fkey ON public.tags USING btree (parent_id);


--
-- Name: fki_xslt_transform_cafe_id_fkey; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_xslt_transform_cafe_id_fkey ON public.xslt_transform USING btree (cafe_id);


--
-- Name: food_dish_IX_food_category_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "food_dish_IX_food_category_id" ON public.dish USING btree (cafe_category_link_id);


--
-- Name: idx_qrtz_ft_job_group; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_job_group ON public.qrtz_fired_triggers USING btree (job_group);


--
-- Name: idx_qrtz_ft_job_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_job_name ON public.qrtz_fired_triggers USING btree (job_name);


--
-- Name: idx_qrtz_ft_job_req_recovery; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_job_req_recovery ON public.qrtz_fired_triggers USING btree (requests_recovery);


--
-- Name: idx_qrtz_ft_trig_group; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_trig_group ON public.qrtz_fired_triggers USING btree (trigger_group);


--
-- Name: idx_qrtz_ft_trig_inst_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_trig_inst_name ON public.qrtz_fired_triggers USING btree (instance_name);


--
-- Name: idx_qrtz_ft_trig_name; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_trig_name ON public.qrtz_fired_triggers USING btree (trigger_name);


--
-- Name: idx_qrtz_ft_trig_nm_gp; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_ft_trig_nm_gp ON public.qrtz_fired_triggers USING btree (sched_name, trigger_name, trigger_group);


--
-- Name: idx_qrtz_j_req_recovery; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_j_req_recovery ON public.qrtz_job_details USING btree (requests_recovery);


--
-- Name: idx_qrtz_t_next_fire_time; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_t_next_fire_time ON public.qrtz_triggers USING btree (next_fire_time);


--
-- Name: idx_qrtz_t_nft_st; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_t_nft_st ON public.qrtz_triggers USING btree (next_fire_time, trigger_state);


--
-- Name: idx_qrtz_t_state; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_qrtz_t_state ON public.qrtz_triggers USING btree (trigger_state);


--
-- Name: role_RoleNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "role_RoleNameIndex" ON public.role USING btree (role_name);


--
-- Name: sms_code_creation_time_ind; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX sms_code_creation_time_ind ON public.sms_code USING btree (creation_time DESC);


--
-- Name: sms_code_is_deleted_ind; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX sms_code_is_deleted_ind ON public.sms_code USING btree (is_deleted);


--
-- Name: user_UserNameIndex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "user_UserNameIndex" ON public."user" USING btree (user_name);


--
-- Name: user_external_login_IX_user_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "user_external_login_IX_user_id" ON public.user_external_login USING btree (user_id);


--
-- Name: user_user_email_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX user_user_email_uindex ON public."user" USING btree (user_email);


--
-- Name: user_external_login FK_public.user_external_login_public.user_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_external_login
    ADD CONSTRAINT "FK_public.user_external_login_public.user_user_id" FOREIGN KEY (user_id) REFERENCES public."user"(id) ON DELETE CASCADE;


--
-- Name: user_role_link FK_public.user_role_link_public.role_role_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_role_link
    ADD CONSTRAINT "FK_public.user_role_link_public.role_role_id" FOREIGN KEY (role_id) REFERENCES public.role(id) ON DELETE CASCADE;


--
-- Name: user_role_link FK_public.user_role_link_public.user_user_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_role_link
    ADD CONSTRAINT "FK_public.user_role_link_public.user_user_id" FOREIGN KEY (user_id) REFERENCES public."user"(id) ON DELETE CASCADE;


--
-- Name: address_company_link address_company_link_address_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.address_company_link
    ADD CONSTRAINT address_company_link_address_id_fkey FOREIGN KEY (address_id) REFERENCES public.address(id);


--
-- Name: address_company_link address_company_link_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.address_company_link
    ADD CONSTRAINT address_company_link_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: bankets bankets_cafe_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankets
    ADD CONSTRAINT bankets_cafe_id_fk FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: bankets bankets_company_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankets
    ADD CONSTRAINT bankets_company_id_fk FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: bankets bankets_menu_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankets
    ADD CONSTRAINT bankets_menu_id_fk FOREIGN KEY (menu_id) REFERENCES public.cafe_menu_patterns(id);


--
-- Name: cafe cafe_cafe_specialization_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe
    ADD CONSTRAINT cafe_cafe_specialization_id_fkey FOREIGN KEY (cafe_specialization_id) REFERENCES public.cafe_specialization(id);


--
-- Name: cafe_category_link cafe_category_link_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_category_link
    ADD CONSTRAINT cafe_category_link_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: cafe_category_link cafe_category_link_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_category_link
    ADD CONSTRAINT cafe_category_link_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.dish_category(id);


--
-- Name: cafe_discount cafe_discount_cafe_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_discount
    ADD CONSTRAINT cafe_discount_cafe_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: cafe_kitchen_link cafe_kitchen_link_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_kitchen_link
    ADD CONSTRAINT cafe_kitchen_link_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: cafe_kitchen_link cafe_kitchen_link_kitchen_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_kitchen_link
    ADD CONSTRAINT cafe_kitchen_link_kitchen_id_fkey FOREIGN KEY (kitchen_id) REFERENCES public.kitchen(id);


--
-- Name: cafe_managers cafe_managers_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_managers
    ADD CONSTRAINT cafe_managers_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: cafe_managers cafe_managers_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_managers
    ADD CONSTRAINT cafe_managers_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: cafe_menu_patterns cafe_menu_patterns_cafe_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_menu_patterns
    ADD CONSTRAINT cafe_menu_patterns_cafe_id_fk FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: cafe_menu_patterns_dishes cafe_menu_patterns_dishes_dish_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_menu_patterns_dishes
    ADD CONSTRAINT cafe_menu_patterns_dishes_dish_id_fk FOREIGN KEY (dish_id) REFERENCES public.dish(id);


--
-- Name: cafe_menu_patterns_dishes cafe_menu_patterns_dishes_pattern_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_menu_patterns_dishes
    ADD CONSTRAINT cafe_menu_patterns_dishes_pattern_id_fk FOREIGN KEY (pattern_id) REFERENCES public.cafe_menu_patterns(id);


--
-- Name: cafe_notification_contacts cafe_notification_contacts_notification_channel_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_notification_contacts
    ADD CONSTRAINT cafe_notification_contacts_notification_channel_id_fkey FOREIGN KEY (notification_channel_id) REFERENCES public.notification_channel(id);


--
-- Name: company_curators company_curators_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_curators
    ADD CONSTRAINT company_curators_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: company_curators company_curators_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_curators
    ADD CONSTRAINT company_curators_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: company_order company_order_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_order
    ADD CONSTRAINT company_order_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: company_order company_order_status_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company_order
    ADD CONSTRAINT company_order_status_fkey FOREIGN KEY (state) REFERENCES public.order_status(id);


--
-- Name: cost_of_delivery cost_of_delivery_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cost_of_delivery
    ADD CONSTRAINT cost_of_delivery_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: discount discount_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.discount
    ADD CONSTRAINT discount_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: discount discount_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.discount
    ADD CONSTRAINT discount_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: discount discount_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.discount
    ADD CONSTRAINT discount_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: dish_in_menu_history dish_in_menu_history_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_in_menu_history
    ADD CONSTRAINT dish_in_menu_history_fk FOREIGN KEY (dish_id) REFERENCES public.dish(id);


--
-- Name: dish food_dish_cafe_category_link_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish
    ADD CONSTRAINT food_dish_cafe_category_link_id_fkey FOREIGN KEY (cafe_category_link_id) REFERENCES public.cafe_category_link(id);


--
-- Name: dish_version food_dish_version_food_dish_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_version
    ADD CONSTRAINT food_dish_version_food_dish_id_fkey FOREIGN KEY (dish_id) REFERENCES public.dish(id);


--
-- Name: dish_in_menu food_schedule_food_dish_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dish_in_menu
    ADD CONSTRAINT food_schedule_food_dish_id_fkey FOREIGN KEY (dish_id) REFERENCES public.dish(id);


--
-- Name: log_message log_message_msg_code_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.log_message
    ADD CONSTRAINT log_message_msg_code_fkey FOREIGN KEY (msg_code) REFERENCES public.log_message_codes(message_code);


--
-- Name: notifications notifications_notification_channel_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_notification_channel_id_fkey FOREIGN KEY (notification_channel_id) REFERENCES public.notification_channel(id);


--
-- Name: notifications notifications_notification_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT notifications_notification_type_id_fkey FOREIGN KEY (notification_type_id) REFERENCES public.notification_type(id);


--
-- Name: order order_banket_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT order_banket_id_fk FOREIGN KEY (banket_id) REFERENCES public.bankets(id);


--
-- Name: order order_company_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT order_company_order_id_fkey FOREIGN KEY (company_order_id) REFERENCES public.company_order(id);


--
-- Name: order order_info_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT order_info_id_fkey FOREIGN KEY (order_info_id) REFERENCES public.order_info(id);


--
-- Name: order_item order_item_food_dish_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_item
    ADD CONSTRAINT order_item_food_dish_id_fkey FOREIGN KEY (dish_id) REFERENCES public.dish(id);


--
-- Name: order_item order_item_order_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.order_item
    ADD CONSTRAINT order_item_order_id_fkey FOREIGN KEY (order_id) REFERENCES public."order"(id);


--
-- Name: cafe_order_notification order_notification_cafe_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_order_notification
    ADD CONSTRAINT order_notification_cafe_fk FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- Name: cafe_order_notification order_notification_user_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cafe_order_notification
    ADD CONSTRAINT order_notification_user_fk FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: order order_status_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT order_status_fkey FOREIGN KEY (state) REFERENCES public.order_status(id);


--
-- Name: order order_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT order_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: order orders_company_orders_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."order"
    ADD CONSTRAINT orders_company_orders_id_fk FOREIGN KEY (company_order_id) REFERENCES public.company_order(id);


--
-- Name: qrtz_blob_triggers qrtz_blob_triggers_sched_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_blob_triggers
    ADD CONSTRAINT qrtz_blob_triggers_sched_name_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES public.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_cron_triggers qrtz_cron_triggers_sched_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_cron_triggers
    ADD CONSTRAINT qrtz_cron_triggers_sched_name_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES public.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_simple_triggers qrtz_simple_triggers_sched_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_simple_triggers
    ADD CONSTRAINT qrtz_simple_triggers_sched_name_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES public.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_simprop_triggers qrtz_simprop_triggers_sched_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_simprop_triggers
    ADD CONSTRAINT qrtz_simprop_triggers_sched_name_fkey FOREIGN KEY (sched_name, trigger_name, trigger_group) REFERENCES public.qrtz_triggers(sched_name, trigger_name, trigger_group) ON DELETE CASCADE;


--
-- Name: qrtz_triggers qrtz_triggers_sched_name_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.qrtz_triggers
    ADD CONSTRAINT qrtz_triggers_sched_name_fkey FOREIGN KEY (sched_name, job_name, job_group) REFERENCES public.qrtz_job_details(sched_name, job_name, job_group);


--
-- Name: scheduled_tasks scheduled_tasks_company_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.scheduled_tasks
    ADD CONSTRAINT scheduled_tasks_company_order_id FOREIGN KEY (company_order_id) REFERENCES public.company_order(id);


--
-- Name: sms_code sms_code_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sms_code
    ADD CONSTRAINT sms_code_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: tag_object_link tag_object_link_tag_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tag_object_link
    ADD CONSTRAINT tag_object_link_tag_id_fkey FOREIGN KEY (tag_id) REFERENCES public.tags(id);


--
-- Name: tags tag_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.tags
    ADD CONSTRAINT tag_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.tags(id);


--
-- Name: CONSTRAINT tag_parent_id_fkey ON tags; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON CONSTRAINT tag_parent_id_fkey ON public.tags IS 'Ключ на существование родителя тега';


--
-- Name: user_company_link user_company_link_company_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_company_link
    ADD CONSTRAINT user_company_link_company_id_fkey FOREIGN KEY (company_id) REFERENCES public.company(id);


--
-- Name: user_company_link user_company_link_default_user_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_company_link
    ADD CONSTRAINT user_company_link_default_user_id_fk FOREIGN KEY (default_address_id) REFERENCES public.address(id);


--
-- Name: user_company_link user_company_link_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_company_link
    ADD CONSTRAINT user_company_link_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.role(id);


--
-- Name: user_company_link user_company_link_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_company_link
    ADD CONSTRAINT user_company_link_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: user user_default_addres_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_default_addres_id_fk FOREIGN KEY (default_address_id) REFERENCES public.address(id) ON DELETE SET NULL;


--
-- Name: user_referral_link user_referral_link_referral_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_referral_link
    ADD CONSTRAINT user_referral_link_referral_id_fkey FOREIGN KEY (referral_id) REFERENCES public."user"(id);


--
-- Name: user_referral_link user_referral_link_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_referral_link
    ADD CONSTRAINT user_referral_link_user_id_fkey FOREIGN KEY (parent_id) REFERENCES public."user"(id);


--
-- Name: user_role_link user_role_link_role_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_role_link
    ADD CONSTRAINT user_role_link_role_id_fk FOREIGN KEY (role_id) REFERENCES public.role(id);


--
-- Name: user_role_link user_role_link_user_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_role_link
    ADD CONSTRAINT user_role_link_user_id_fk FOREIGN KEY (user_id) REFERENCES public."user"(id);


--
-- Name: xslt_to_cafe xslt_to_cafe_cafe_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_to_cafe
    ADD CONSTRAINT xslt_to_cafe_cafe_fk FOREIGN KEY (id_cafe) REFERENCES public.cafe(id);


--
-- Name: xslt_to_cafe xslt_to_cafe_xslt_transform_fk; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_to_cafe
    ADD CONSTRAINT xslt_to_cafe_xslt_transform_fk FOREIGN KEY (xslt_id) REFERENCES public.xslt_transform(id);


--
-- Name: xslt_transform xslt_transform_cafe_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.xslt_transform
    ADD CONSTRAINT xslt_transform_cafe_id_fkey FOREIGN KEY (cafe_id) REFERENCES public.cafe(id);


--
-- PostgreSQL database dump complete
--

