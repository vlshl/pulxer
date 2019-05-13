--
-- PostgreSQL database dump
--

-- Dumped from database version 11.1
-- Dumped by pg_dump version 11.1

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- Name: account; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.account (
    account_id integer NOT NULL,
    code character varying(50) NOT NULL,
    name text NOT NULL,
    comm_perc numeric(18,5) NOT NULL,
    short_enable boolean NOT NULL,
    account_type smallint NOT NULL
);


ALTER TABLE public.account OWNER TO postgres;

--
-- Name: account_account_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.account_account_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.account_account_id_seq OWNER TO postgres;

--
-- Name: account_account_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.account_account_id_seq OWNED BY public.account.account_id;


--
-- Name: barhistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.barhistory (
    insstore_id integer NOT NULL,
    bar_time integer NOT NULL,
    open integer NOT NULL,
    close_d smallint NOT NULL,
    high_d smallint NOT NULL,
    low_d smallint NOT NULL,
    volume integer NOT NULL
);


ALTER TABLE public.barhistory OWNER TO postgres;

--
-- Name: cash; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cash (
    cash_id integer NOT NULL,
    initial numeric(18,5) NOT NULL,
    account_id integer NOT NULL,
    current numeric(18,5) NOT NULL,
    sell numeric(18,5) NOT NULL,
    buy numeric(18,5) NOT NULL,
    sell_comm numeric(18,5) NOT NULL,
    buy_comm numeric(18,5) NOT NULL
);


ALTER TABLE public.cash OWNER TO postgres;

--
-- Name: cash_cash_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.cash_cash_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.cash_cash_id_seq OWNER TO postgres;

--
-- Name: cash_cash_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.cash_cash_id_seq OWNED BY public.cash.cash_id;


--
-- Name: dbversion; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.dbversion (
    version integer
);


ALTER TABLE public.dbversion OWNER TO postgres;

--
-- Name: freedays; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.freedays (
    insstore_id integer NOT NULL,
    date date NOT NULL
);


ALTER TABLE public.freedays OWNER TO postgres;

--
-- Name: holding; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.holding (
    holding_id integer NOT NULL,
    ins_id integer NOT NULL,
    lots integer NOT NULL,
    account_id integer NOT NULL
);


ALTER TABLE public.holding OWNER TO postgres;

--
-- Name: holding_holding_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.holding_holding_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.holding_holding_id_seq OWNER TO postgres;

--
-- Name: holding_holding_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.holding_holding_id_seq OWNED BY public.holding.holding_id;


--
-- Name: insstore; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.insstore (
    insstore_id integer NOT NULL,
    ins_id integer NOT NULL,
    tf smallint NOT NULL,
    enable boolean NOT NULL
);


ALTER TABLE public.insstore OWNER TO postgres;

--
-- Name: ins_store_ins_store_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.ins_store_ins_store_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.ins_store_ins_store_id_seq OWNER TO postgres;

--
-- Name: ins_store_ins_store_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.ins_store_ins_store_id_seq OWNED BY public.insstore.insstore_id;


--
-- Name: instrum; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.instrum (
    ins_id integer NOT NULL,
    ticker character varying(50) NOT NULL,
    short_name character varying(50) NOT NULL,
    name character varying(1000) NOT NULL,
    lot_size integer NOT NULL,
    decimals integer NOT NULL,
    price_step numeric NOT NULL
);


ALTER TABLE public.instrum OWNER TO postgres;

--
-- Name: instrum_ins_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.instrum_ins_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.instrum_ins_id_seq OWNER TO postgres;

--
-- Name: instrum_ins_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.instrum_ins_id_seq OWNED BY public.instrum.ins_id;


--
-- Name: orders; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orders (
    order_id integer NOT NULL,
    order_time timestamp without time zone NOT NULL,
    ins_id integer NOT NULL,
    buy_sell smallint NOT NULL,
    lots integer NOT NULL,
    price numeric(18,5),
    status smallint NOT NULL,
    account_id integer NOT NULL,
    stoporder_id integer,
    order_no bigint NOT NULL
);


ALTER TABLE public.orders OWNER TO postgres;

--
-- Name: order_order_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.order_order_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.order_order_id_seq OWNER TO postgres;

--
-- Name: order_order_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.order_order_id_seq OWNED BY public.orders.order_id;


--
-- Name: periods; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.periods (
    insstore_id integer NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    last_dirty boolean NOT NULL
);


ALTER TABLE public.periods OWNER TO postgres;

--
-- Name: positions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.positions (
    pos_id integer NOT NULL,
    ins_id integer NOT NULL,
    count integer NOT NULL,
    open_time timestamp without time zone NOT NULL,
    open_price numeric(18,5) NOT NULL,
    close_time timestamp without time zone,
    close_price numeric(18,5),
    pos_type smallint NOT NULL,
    account_id integer NOT NULL
);


ALTER TABLE public.positions OWNER TO postgres;

--
-- Name: position_pos_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.position_pos_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.position_pos_id_seq OWNER TO postgres;

--
-- Name: position_pos_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.position_pos_id_seq OWNED BY public.positions.pos_id;


--
-- Name: postrade; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.postrade (
    pos_id integer NOT NULL,
    trade_id integer NOT NULL
);


ALTER TABLE public.postrade OWNER TO postgres;

--
-- Name: replication; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.replication (
    local_id integer NOT NULL,
    remote_id integer NOT NULL,
    repl_object integer NOT NULL
);


ALTER TABLE public.replication OWNER TO postgres;

--
-- Name: repository; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.repository (
    repos_id integer NOT NULL,
    key text NOT NULL,
    data text NOT NULL
);


ALTER TABLE public.repository OWNER TO postgres;

--
-- Name: repository_repos_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.repository_repos_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.repository_repos_id_seq OWNER TO postgres;

--
-- Name: repository_repos_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.repository_repos_id_seq OWNED BY public.repository.repos_id;


--
-- Name: stoporder; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.stoporder (
    stoporder_id integer NOT NULL,
    stoporder_time timestamp without time zone NOT NULL,
    ins_id integer NOT NULL,
    buy_sell smallint NOT NULL,
    stop_type smallint NOT NULL,
    end_time timestamp without time zone,
    alert_price numeric(18,5) NOT NULL,
    price numeric(18,5),
    lots integer NOT NULL,
    status smallint NOT NULL,
    account_id integer NOT NULL,
    complete_time timestamp without time zone,
    stoporder_no bigint NOT NULL
);


ALTER TABLE public.stoporder OWNER TO postgres;

--
-- Name: stop_order_stop_order_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.stop_order_stop_order_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.stop_order_stop_order_id_seq OWNER TO postgres;

--
-- Name: stop_order_stop_order_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.stop_order_stop_order_id_seq OWNED BY public.stoporder.stoporder_id;


--
-- Name: testconfig; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.testconfig (
    testconfig_id integer NOT NULL,
    name text NOT NULL,
    data text NOT NULL
);


ALTER TABLE public.testconfig OWNER TO postgres;

--
-- Name: testconfig_testconfig_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.testconfig_testconfig_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.testconfig_testconfig_id_seq OWNER TO postgres;

--
-- Name: testconfig_testconfig_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.testconfig_testconfig_id_seq OWNED BY public.testconfig.testconfig_id;


--
-- Name: tickhistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tickhistory (
    tickhistory_id integer NOT NULL,
    hist_date date NOT NULL,
    ins_id integer NOT NULL,
    data bytea NOT NULL
);


ALTER TABLE public.tickhistory OWNER TO postgres;

--
-- Name: tickhistory_tickhistory_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tickhistory_tickhistory_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tickhistory_tickhistory_id_seq OWNER TO postgres;

--
-- Name: tickhistory_tickhistory_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tickhistory_tickhistory_id_seq OWNED BY public.tickhistory.tickhistory_id;


--
-- Name: ticksource; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ticksource (
    ticksource_id integer NOT NULL,
    name text NOT NULL,
    data text NOT NULL
);


ALTER TABLE public.ticksource OWNER TO postgres;

--
-- Name: ticksource_ticksource_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.ticksource_ticksource_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.ticksource_ticksource_id_seq OWNER TO postgres;

--
-- Name: ticksource_ticksource_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.ticksource_ticksource_id_seq OWNED BY public.ticksource.ticksource_id;


--
-- Name: trade; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.trade (
    trade_id integer NOT NULL,
    orders_id integer NOT NULL,
    trade_time timestamp without time zone NOT NULL,
    ins_id integer NOT NULL,
    buy_sell smallint NOT NULL,
    lots integer NOT NULL,
    price numeric(18,5) NOT NULL,
    account_id integer NOT NULL,
    comm numeric(18,5) NOT NULL,
    trade_no bigint NOT NULL
);


ALTER TABLE public.trade OWNER TO postgres;

--
-- Name: trade_trade_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.trade_trade_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.trade_trade_id_seq OWNER TO postgres;

--
-- Name: trade_trade_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.trade_trade_id_seq OWNED BY public.trade.trade_id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    login character varying(50) NOT NULL,
    pwd_hash character varying(50) NOT NULL,
    role character varying(50) NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.users_user_id_seq OWNER TO postgres;

--
-- Name: users_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;


--
-- Name: account account_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.account ALTER COLUMN account_id SET DEFAULT nextval('public.account_account_id_seq'::regclass);


--
-- Name: cash cash_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash ALTER COLUMN cash_id SET DEFAULT nextval('public.cash_cash_id_seq'::regclass);


--
-- Name: holding holding_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.holding ALTER COLUMN holding_id SET DEFAULT nextval('public.holding_holding_id_seq'::regclass);


--
-- Name: insstore insstore_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.insstore ALTER COLUMN insstore_id SET DEFAULT nextval('public.ins_store_ins_store_id_seq'::regclass);


--
-- Name: instrum ins_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instrum ALTER COLUMN ins_id SET DEFAULT nextval('public.instrum_ins_id_seq'::regclass);


--
-- Name: orders order_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders ALTER COLUMN order_id SET DEFAULT nextval('public.order_order_id_seq'::regclass);


--
-- Name: positions pos_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.positions ALTER COLUMN pos_id SET DEFAULT nextval('public.position_pos_id_seq'::regclass);


--
-- Name: repository repos_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.repository ALTER COLUMN repos_id SET DEFAULT nextval('public.repository_repos_id_seq'::regclass);


--
-- Name: stoporder stoporder_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stoporder ALTER COLUMN stoporder_id SET DEFAULT nextval('public.stop_order_stop_order_id_seq'::regclass);


--
-- Name: testconfig testconfig_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.testconfig ALTER COLUMN testconfig_id SET DEFAULT nextval('public.testconfig_testconfig_id_seq'::regclass);


--
-- Name: tickhistory tickhistory_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickhistory ALTER COLUMN tickhistory_id SET DEFAULT nextval('public.tickhistory_tickhistory_id_seq'::regclass);


--
-- Name: ticksource ticksource_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticksource ALTER COLUMN ticksource_id SET DEFAULT nextval('public.ticksource_ticksource_id_seq'::regclass);


--
-- Name: trade trade_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trade ALTER COLUMN trade_id SET DEFAULT nextval('public.trade_trade_id_seq'::regclass);


--
-- Name: users user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);


--
-- Name: account pk_account; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.account
    ADD CONSTRAINT pk_account PRIMARY KEY (account_id);


--
-- Name: cash pk_cash; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash
    ADD CONSTRAINT pk_cash PRIMARY KEY (cash_id);


--
-- Name: holding pk_holding; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.holding
    ADD CONSTRAINT pk_holding PRIMARY KEY (holding_id);


--
-- Name: insstore pk_insstore; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.insstore
    ADD CONSTRAINT pk_insstore PRIMARY KEY (insstore_id);


--
-- Name: instrum pk_instrum; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.instrum
    ADD CONSTRAINT pk_instrum PRIMARY KEY (ins_id);


--
-- Name: orders pk_order; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT pk_order PRIMARY KEY (order_id);


--
-- Name: positions pk_positions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT pk_positions PRIMARY KEY (pos_id);


--
-- Name: repository pk_repository; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.repository
    ADD CONSTRAINT pk_repository PRIMARY KEY (repos_id);


--
-- Name: stoporder pk_stoporder; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stoporder
    ADD CONSTRAINT pk_stoporder PRIMARY KEY (stoporder_id);


--
-- Name: testconfig pk_testconfig; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.testconfig
    ADD CONSTRAINT pk_testconfig PRIMARY KEY (testconfig_id);


--
-- Name: tickhistory pk_tickhistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickhistory
    ADD CONSTRAINT pk_tickhistory PRIMARY KEY (tickhistory_id);


--
-- Name: ticksource pk_ticksource; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ticksource
    ADD CONSTRAINT pk_ticksource PRIMARY KEY (ticksource_id);


--
-- Name: trade pk_trade; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trade
    ADD CONSTRAINT pk_trade PRIMARY KEY (trade_id);


--
-- Name: users pk_users; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT pk_users PRIMARY KEY (user_id);


--
-- Name: barhistory un_barhistory_insstoreid_bartime; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.barhistory
    ADD CONSTRAINT un_barhistory_insstoreid_bartime UNIQUE (insstore_id, bar_time);


--
-- Name: freedays un_freedays_insstoreid_date; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.freedays
    ADD CONSTRAINT un_freedays_insstoreid_date UNIQUE (insstore_id, date);


--
-- Name: insstore un_insstore_insid_tf; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.insstore
    ADD CONSTRAINT un_insstore_insid_tf UNIQUE (ins_id, tf);


--
-- Name: ix_cash_accountid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_cash_accountid ON public.cash USING btree (account_id);

ALTER TABLE public.cash CLUSTER ON ix_cash_accountid;


--
-- Name: ix_holding_accountid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_holding_accountid ON public.holding USING btree (account_id);


--
-- Name: ix_insid_histdate; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_insid_histdate ON public.tickhistory USING btree (ins_id, hist_date);


--
-- Name: ix_instrum_ticker; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_instrum_ticker ON public.instrum USING btree (ticker);


--
-- Name: ix_order_accountid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_order_accountid ON public.orders USING btree (account_id);


--
-- Name: ix_period_insstoreid_startdate; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_period_insstoreid_startdate ON public.periods USING btree (insstore_id, start_date);


--
-- Name: ix_positions_accountid_closetime; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_positions_accountid_closetime ON public.positions USING btree (account_id, close_time);


--
-- Name: ix_postrade_pos; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_postrade_pos ON public.postrade USING btree (pos_id);


--
-- Name: ix_replication_replobject; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_replication_replobject ON public.replication USING btree (repl_object);


--
-- Name: ix_repository_key; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_repository_key ON public.repository USING btree (key);


--
-- Name: ix_stoporder_accountid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_stoporder_accountid ON public.stoporder USING btree (account_id);


--
-- Name: ix_trade_accountid; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_trade_accountid ON public.trade USING btree (account_id);


--
-- Name: ix_users_login; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX ix_users_login ON public.users USING btree (login);


--
-- Name: holding fk_account; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.holding
    ADD CONSTRAINT fk_account FOREIGN KEY (account_id) REFERENCES public.account(account_id);


--
-- Name: stoporder fk_account; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stoporder
    ADD CONSTRAINT fk_account FOREIGN KEY (account_id) REFERENCES public.account(account_id);


--
-- Name: orders fk_account; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT fk_account FOREIGN KEY (account_id) REFERENCES public.account(account_id);


--
-- Name: trade fk_account; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trade
    ADD CONSTRAINT fk_account FOREIGN KEY (account_id) REFERENCES public.account(account_id);


--
-- Name: positions fk_account; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT fk_account FOREIGN KEY (account_id) REFERENCES public.account(account_id);


--
-- Name: barhistory fk_barhistory_insstore; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.barhistory
    ADD CONSTRAINT fk_barhistory_insstore FOREIGN KEY (insstore_id) REFERENCES public.insstore(insstore_id);


--
-- Name: cash fk_cash_account; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cash
    ADD CONSTRAINT fk_cash_account FOREIGN KEY (account_id) REFERENCES public.account(account_id);


--
-- Name: freedays fk_insstore; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.freedays
    ADD CONSTRAINT fk_insstore FOREIGN KEY (insstore_id) REFERENCES public.insstore(insstore_id);


--
-- Name: periods fk_insstore; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.periods
    ADD CONSTRAINT fk_insstore FOREIGN KEY (insstore_id) REFERENCES public.insstore(insstore_id);


--
-- Name: insstore fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.insstore
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: holding fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.holding
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: stoporder fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.stoporder
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: orders fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: trade fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trade
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: tickhistory fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickhistory
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: positions fk_instrum; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.positions
    ADD CONSTRAINT fk_instrum FOREIGN KEY (ins_id) REFERENCES public.instrum(ins_id);


--
-- Name: trade fk_orders; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trade
    ADD CONSTRAINT fk_orders FOREIGN KEY (orders_id) REFERENCES public.orders(order_id);


--
-- Name: postrade fk_positions; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.postrade
    ADD CONSTRAINT fk_positions FOREIGN KEY (pos_id) REFERENCES public.positions(pos_id);


--
-- Name: orders fk_stoporder; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orders
    ADD CONSTRAINT fk_stoporder FOREIGN KEY (stoporder_id) REFERENCES public.stoporder(stoporder_id);


--
-- Name: postrade fk_trade; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.postrade
    ADD CONSTRAINT fk_trade FOREIGN KEY (trade_id) REFERENCES public.trade(trade_id);


--
-- PostgreSQL database dump complete
--

