FROM docker.elastic.co/elasticsearch/elasticsearch:5.3.0
MAINTAINER GluteusMaximus
RUN ls /usr/share/elasticsearch/bin
RUN export http_proxy="http://172.30.20.128:3128/"
#RUN export ES_JAVA_OPTS="-Dhttp.proxyHost=172.30.20.128 -Dhttp.proxyPort=3128"; /usr/share/elasticsearch/bin/elasticsearch-plugin install --batch analysis-phonetic 
#RUN sysctl -w vm.max_map_count=262144
USER elasticsearch
