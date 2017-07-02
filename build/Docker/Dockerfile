FROM docker.elastic.co/elasticsearch/elasticsearch:5.0.2
ADD elasticsearch.yml /usr/share/elasticsearch/config/
USER root
chown elasticsearch:elasticsearch config/elasticsearch.yml
RUN /usr/share/elasticsearch/bin/plugin install --batch analysis-phonetic
USER elasticsearch
