FROM docker.elastic.co/elasticsearch/elasticsearch:5.3.0
MAINTAINER GluteusMaximus
RUN ls /usr/share/elasticsearch/bin
RUN /usr/share/elasticsearch/bin/elasticsearch-plugin install --batch analysis-phonetic
#RUN sysctl -w vm.max_map_count=262144
USER elasticsearch
