FROM node:20-alpine
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install

COPY . .

EXPOSE 3000
CMD ["npx", "vite", "--port", "3000", "--host", "0.0.0.0"]
