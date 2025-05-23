FROM gitpod/workspace-full:latest

USER root
# 1) Add MS package feed
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb

# 2) Install .NET SDK
RUN apt-get update \
    && apt-get install -y dotnet-sdk-8.0 \
    && rm -rf /var/lib/apt/lists/*

USER gitpod
WORKDIR /workspace
CMD ["bash"]
