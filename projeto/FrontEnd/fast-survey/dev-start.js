#!/usr/bin/env node

const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🚀 Iniciando servidor de desenvolvimento com limpeza automática...');

// Função para limpar cache
function clearCache() {
  console.log('🧹 Limpando cache...');
  
  const cachePaths = [
    'node_modules/.cache',
    'build',
    '.next',
    '.nuxt',
    '.cache'
  ];

  cachePaths.forEach(cachePath => {
    const fullPath = path.join(process.cwd(), cachePath);
    
    if (fs.existsSync(fullPath)) {
      try {
        if (fs.lstatSync(fullPath).isDirectory()) {
          fs.rmSync(fullPath, { recursive: true, force: true });
          console.log(`✅ Removido: ${cachePath}`);
        }
      } catch (error) {
        console.log(`⚠️ Erro ao remover ${cachePath}:`, error.message);
      }
    }
  });
  
  console.log('✅ Cache limpo!');
}

// Limpa cache antes de iniciar
clearCache();

// Inicia o servidor React
const reactProcess = spawn('npm', ['start'], {
  stdio: 'inherit',
  shell: true
});

// Captura sinais de encerramento
process.on('SIGINT', () => {
  console.log('\n🛑 Encerrando servidor...');
  reactProcess.kill('SIGINT');
  
  // Aguarda um pouco e limpa o cache
  setTimeout(() => {
    console.log('🧹 Limpando cache após encerramento...');
    clearCache();
    process.exit(0);
  }, 1000);
});

process.on('SIGTERM', () => {
  console.log('\n🛑 Encerrando servidor...');
  reactProcess.kill('SIGTERM');
  
  setTimeout(() => {
    console.log('🧹 Limpando cache após encerramento...');
    clearCache();
    process.exit(0);
  }, 1000);
});

// Captura erros
reactProcess.on('error', (error) => {
  console.error('❌ Erro ao iniciar servidor:', error);
  process.exit(1);
});

reactProcess.on('exit', (code) => {
  console.log(`\n🛑 Servidor encerrado com código: ${code}`);
  console.log('🧹 Limpando cache após encerramento...');
  clearCache();
  process.exit(code);
});
