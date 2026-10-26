# Start from an image that already has Bun installed
FROM oven/bun:1
WORKDIR /app

# Install the packages first, so Docker can reuse this step if only the code changes
COPY package.json bun.lock ./
RUN bun install --frozen-lockfile --production

# Copy the rest of the code
COPY . .

ENV NODE_ENV=production
ENV PORT=8001

# Run as the "bun" user instead of root, for safety
USER bun

EXPOSE 8001
CMD ["bun", "src/index.ts"]
