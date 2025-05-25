#!/usr/bin/env bash
declare -A PORTS=(
  [search]=5001 [inventory]=5002 [routing]=5003 [recommendation]=5004
  [pricing]=5005 [analytics]=5006 [cache]=5007 [auth]=5008
  [cart]=5009 [order]=5010
)

for svc in "${!PORTS[@]}"; do
  port=${PORTS[$svc]}
  status=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:$port/health)
  printf "%-14s → %s\n" "$svc" "$status"
done
