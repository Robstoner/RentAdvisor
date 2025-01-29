FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

# Install NodeJs
RUN apt-get -y update \
    && apt-get install -y curl \
    && curl -sL https://deb.nodesource.com/setup_18.x | bash - \ 
    && apt-get install -y nodejs \
    && apt-get clean

ENV ASPNETCORE_URLS=http://+:8080

WORKDIR /app
COPY ./rentadvisor.client /app

RUN npm install

ENV NODE_ENV=production

RUN npm run build

FROM ubuntu

RUN apt-get update
RUN apt-get install nginx -y

COPY --from=build /app/dist /var/www/html/

EXPOSE 80

CMD ["nginx","-g","daemon off;"]