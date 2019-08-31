FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS builder
WORKDIR /app

# caches restore result by copying csproj file separately
COPY Application/*.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish --output /app/ --configuration Release
RUN sed -n 's:.*<AssemblyName>\(.*\)</AssemblyName>.*:\1:p' *.csproj > __assemblyname
RUN if [ ! -s __assemblyname ]; then filename=$(ls *.csproj); echo ${filename%.*} > __assemblyname; fi

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine AS runtime
WORKDIR /app
COPY --from=builder /app .

ENV PORT 80
EXPOSE 80

ENTRYPOINT dotnet $(cat /app/__assemblyname).dll
