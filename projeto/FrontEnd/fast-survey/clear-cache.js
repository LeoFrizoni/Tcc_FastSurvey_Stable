#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

console.log('🧹 Limpando cache do React...');

const cachePaths = [
  'node_modules/.cache',
  'build',
  '.next',
  '.nuxt',
  '.cache'
];

let cleanedCount = 0;

cachePaths.forEach(cachePath => {
  const fullPath = path.join(process.cwd(), cachePath);
  
  if (fs.existsSync(fullPath)) {
    try {
      if (fs.lstatSync(fullPath).isDirectory()) {
        fs.rmSync(fullPath, { recursive: true, force: true });
        console.log(`✅ Removido: ${cachePath}`);
        cleanedCount++;
      }
    } catch (error) {
      console.log(`⚠️ Erro ao remover ${cachePath}:`, error.message);
    }
  } else {
    console.log(`ℹ️ Não encontrado: ${cachePath}`);
  }
});

console.log(`\n🎉 Cache limpo! ${cleanedCount} diretórios removidos.`);
console.log('💡 Dica: Execute "npm start" para reiniciar o servidor.');
