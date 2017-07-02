FROM mono:latest
MAINTAINER GluteusMaximus

# copy binaries
ADD ./bin usr

ADD ./wait-for-it.sh usr

# Create app directory
#RUN mkdir -p /usr/Data
#VOLUME ["/usr/Data"]

RUN chmod +x /usr/wait-for-it.sh

# default environment variables
WORKDIR /usr

## wait for activemq and sql to start
ENTRYPOINT [ "./wait-for-it.sh", "activemq:61616", "--", "./wait-for-it.sh", "database:1433", "--", "mono", "Quest.Cmd.exe" ]
