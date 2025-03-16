create table public.outbox (
    id serial,
    topic text,
    key text,
    payload text,
    state integer
);

create index outbox_id_topic on outbox (id, topic);
create index outbox_state_1_id on outbox (state, id) where state = 1;
create index outbox_state_0_id on outbox (state, id) where state = 0;

create table worker_task (
    id serial primary key,
    topic text,
    lease_end timestamp with time zone,
    batch_size integer,
    delay_milliseconds integer
);

insert into worker_task (topic, lease_end, batch_size, delay_milliseconds)
select 'test' || i%5 + 1, now() - interval '1 day', 500, 10
FROM generate_series(1, 5) AS t(i);

INSERT INTO outbox(topic, key, payload, state)
SELECT 'test' || i%5 + 1, i::text, i::text, 0
FROM generate_series(1, 1000000) AS t(i);