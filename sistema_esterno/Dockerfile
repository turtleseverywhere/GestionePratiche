### STAGE 1: Build ###
FROM node:18.16.1-alpine3.17 AS build
WORKDIR /usr/src/app
COPY . .
RUN npm install
RUN npm run build

### STAGE 2: Run ###
FROM nginx:1.25.1-alpine
COPY default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /usr/src/app/dist/sistema_esterno/browser /usr/share/nginx/html
EXPOSE 80
