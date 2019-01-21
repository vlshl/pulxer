echo Recreate empty testdb
psql -f recreate_empty_testdb.sql

echo Create testdb structure
psql -d pulxer_test -f createdb.sql

pause


