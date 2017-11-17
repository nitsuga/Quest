FROM docker.elastic.co/elasticsearch/elasticsearch:5.3.0
MAINTAINER GluteusMaximus
RUN /usr/share/elasticsearch/bin/elasticsearch-plugin install --batch analysis-phonetic
# you must run this on the docker host
# sysctl -w vm.max_map_count=262144
USER elasticsearch
