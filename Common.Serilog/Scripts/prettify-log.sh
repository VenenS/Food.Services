#!/bin/bash

JQ_WITH_FIELDS_FILTER='[
    ["\(.ts | sub("T"; " ") | sub("Z"; "")) [\(.lvl)] \(" " * (5 - (.lvl | length)) + "") \(.msg)"],
    (del(.["msg", "ts", "lvl"])
        | select(length > 0)
        | del(.["rq.userAgent"])
        | to_entries
        | ([map(select(.key != "props") | "\(.key)='\''\(.value | tostring)'\''"),
            map(select(.key == "props") | .value | map("\(.key)='\''\(.value | tostring)'\''"))])
    )
] | flatten | map(select(. != null)) | join(" ")
'
JQ_QUIET_FILTER='
    "\(.ts | sub("T"; " ") | sub("Z"; "")) [\(.lvl)] \(" " * (5 - (.lvl | length)) + "") \(.msg)"
'
JQ_FILTER="$JQ_WITH_FIELDS_FILTER"
FOLLOW_TAIL=0

function print_usage_and_die {
    echo "Usage: $(basename "${BASH_SOURCE[0]}") [-f] FILE" > /dev/stderr
    echo '' > /dev/stderr
    echo 'Pretty-prints JSON logs produced by Common.Serilog' > /dev/stderr
    echo '' > /dev/stderr
    echo '  -f    Follow output, same as "tail -f"' > /dev/stderr
    echo "  -q    Quiet mode (don't output extra fields)" > /dev/stderr
    exit 1
}

while getopts "fqh" OPT; do
    case "$OPT" in
        f)
            FOLLOW_TAIL=1
            ;;
        q)
            JQ_FILTER="$JQ_QUIET_FILTER"
            ;;
        *)
            print_usage_and_die
            ;;
    esac
done

FILE="${@:$OPTIND:1}"

if [[ -z "$FILE" ]]; then
    print_usage_and_die
fi

if ! which jq > /dev/null; then
    "ERROR: jq was not found in PATH" > /dev/stderr
    exit 1;
fi

if [[ $FOLLOW_TAIL == 0 ]]; then
    cat "$FILE" | jq -rc "$JQ_FILTER"
else
    tail -f "$FILE" | jq -rc "$JQ_FILTER"
fi