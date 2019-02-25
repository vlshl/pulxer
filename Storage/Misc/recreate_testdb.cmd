echo Recreate empty testdb
psql -U postgres -f recreate_empty_testdb.sql

echo Create testdb structure
psql -U postgres -d pulxer_test -f createdb.sql

pause


