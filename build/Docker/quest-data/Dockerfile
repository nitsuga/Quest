FROM microsoft/mssql-server-linux:2017-GA

ENV ACCEPT_EULA Y
ENV SA_PASSWORD M3Gurdy*

EXPOSE 1433

#RUN apt-get update && apt-get install -y vim

# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# Bundle app source
COPY attachdata.sh /usr/src/app
COPY setup.sql /usr/src/app
COPY entrypoint.sh /usr/src/app

# Grant permissions for the attach-data script to be executable
RUN chmod +x /usr/src/app/attachdata.sh
RUN chmod +x /usr/src/app/entrypoint.sh

CMD /bin/bash ./entrypoint.sh
