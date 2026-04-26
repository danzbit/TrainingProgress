# Build stage
FROM node:20-alpine AS builder

WORKDIR /app

# Copy dependency manifests first for better layer caching
COPY package.json package-lock.json ./

# Install dependencies
RUN npm install

# Copy the rest of the project files
COPY . .

# Build configuration
ARG VITE_API_BASE_URL=
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
ENV NODE_ENV=production

# Build the application
RUN npm run build

# Runtime stage
FROM nginx:alpine

# Copy nginx config
COPY --from=builder /app/nginx.conf /etc/nginx/nginx.conf

# Copy built app from builder
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
