FROM mono:latest
MAINTAINER GluteusMaximus

# copy binaries
ADD ./bin/Debug usr

ADD ./wait-for-it.sh usr

RUN chmod +x /usr/wait-for-it.sh

WORKDIR /usr

ENTRYPOINT [ "./wait-for-it.sh", "activemq:61616", "--", "./wait-for-it.sh", "database:1433", "--", "mono", "Quest.Cmd.exe" ]
#CMD [ "/usr/Data/CmdConfig/eisec.json" ]
