const { initializeDatabase } = require('./database');
const fs = require('fs');
const path = require('path');

async function setup() {
  try {
    // Create data directory if it doesn't exist
    const dataDir = path.join(__dirname, '../../data');
    if (!fs.existsSync(dataDir)) {
      fs.mkdirSync(dataDir, { recursive: true });
      console.log('Created data directory');
    }

    // Initialize database
    await initializeDatabase();
    console.log('Database setup completed successfully');
    process.exit(0);
  } catch (error) {
    console.error('Database setup failed:', error);
    process.exit(1);
  }
}

if (require.main === module) {
  setup();
}
