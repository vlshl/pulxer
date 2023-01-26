-- SEQUENCE: public.device_device_uid_seq

-- DROP SEQUENCE public.device_device_uid_seq;

CREATE SEQUENCE public.device_device_uid_seq;

ALTER SEQUENCE public.device_device_uid_seq
    OWNER TO postgres;


-- Table: public.device

-- DROP TABLE public.device;

CREATE TABLE public.device
(
    device_id integer NOT NULL DEFAULT nextval('device_device_uid_seq'::regclass),
    code character varying COLLATE pg_catalog."default" NOT NULL,
    user_id integer NOT NULL,
    uid character varying COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_device PRIMARY KEY (device_id),
    CONSTRAINT fk_device_user FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.device
    OWNER to postgres;


