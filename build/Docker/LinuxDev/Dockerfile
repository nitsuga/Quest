FROM ubuntu:latest
RUN apt-get update
RUN apt-get dist-upgrade -y
RUN apt-get update

RUN adduser --disabled-password --gecos "" dockerx 
RUN adduser dockerx sudo
RUN adduser dockerx users
RUN echo "dockerx:docker" | chpasswd

RUN apt-get install -y xterm
RUN apt-get install -y xrdp 

RUN apt-get install -y software-properties-common python-software-properties

ENV DEBIAN_FRONTEND noninteractive

#RUN install-keymap en
#RUN add-apt-repository ppa:gnome3-team/gnome3
#RUN apt install -y gnome gnome-shell
#RUN apt-get install -y --no-install-recommends ubuntu-desktop
#RUN apt-get install -y gnome-panel
#RUN apt-get install -y unity-lens-applications
#RUN gconftool -s /apps/gnome-terminal/profiles/Default/use_system_font -t bool false

#### for Mate desktop
RUN apt-get install -y mate-desktop
RUN apt-get install -y mate-desktop-environment-extras

#RUN \
#	apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
#	apt-get update && \
#	apt-get install -y mono-complete 


RUN add-apt-repository ppa:alexlarsson/flatpak
RUN apt update
RUN apt install -y flatpak	
	
#RUN apt-get install -y monodevelop


#RUN apt-get install -y chromium-browser

RUN echo "/etc/init.d/xrdp start && tail -f /var/log/* " > start.sh
RUN chmod +x ./start.sh

CMD "./start.sh"
EXPOSE 3389

