CREATE SEQUENCE public.global_global_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

ALTER SEQUENCE public.global_global_id_seq
    OWNER TO postgres;


CREATE SEQUENCE public.setting_setting_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

ALTER SEQUENCE public.setting_setting_id_seq
    OWNER TO postgres;



CREATE TABLE public.global
(
    global_id integer NOT NULL DEFAULT nextval('global_global_id_seq'::regclass),
    key character varying COLLATE pg_catalog."default" NOT NULL,
    value character varying COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_global PRIMARY KEY (global_id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.global
    OWNER to postgres;



CREATE TABLE public.setting
(
    setting_id integer NOT NULL DEFAULT nextval('setting_setting_id_seq'::regclass),
    user_id integer NOT NULL,
    category character varying COLLATE pg_catalog."default" NOT NULL,
    value character varying COLLATE pg_catalog."default" NOT NULL,
    key character varying COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_setting PRIMARY KEY (setting_id),
    CONSTRAINT fk_setting_users FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.setting
    OWNER to postgres;


