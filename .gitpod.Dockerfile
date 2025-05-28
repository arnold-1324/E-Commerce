FROM gitpod/workspace-full:latest

# Install .NET SDK using the official install script
RUN curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir /usr/share/dotnet
RUN ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Optional: Verify installation
RUN dotnet --info

USER gitpod
WORKDIR /workspace