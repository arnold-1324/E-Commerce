#!/bin/sh
# wait-for.sh: Wait for a host and port to be available

set -e

host="$1"
port="$2"
shift 2

until nc -z "$host" "$port"; do
  echo "Waiting for $host:$port..."
  sleep 2
done

exec "$@"
