curl -XPUT -u elastic 'http://127.0.0.1:9200/_xpack/license?acknowledge=true' -H "Content-Type: application/json" -d @license.json
