// Debug script para testar o token JWT
// Execute no console do navegador

function debugToken() {
  console.log('🔍 === DEBUG DO TOKEN JWT ===');
  
  // 1. Verificar se o token existe
  const token = localStorage.getItem('token');
  console.log('1. Token existe:', !!token);
  console.log('2. Token length:', token?.length);
  console.log('3. Token preview:', token?.substring(0, 50) + '...');
  
  if (!token) {
    console.log('❌ Nenhum token encontrado no localStorage');
    return;
  }
  
  // 2. Verificar estrutura do token
  const parts = token.split('.');
  console.log('4. Número de partes:', parts.length);
  
  if (parts.length !== 3) {
    console.log('❌ Token não tem 3 partes (header.payload.signature)');
    return;
  }
  
  // 3. Decodificar header
  try {
    const header = JSON.parse(atob(parts[0]));
    console.log('5. Header:', header);
  } catch (error) {
    console.log('❌ Erro ao decodificar header:', error);
  }
  
  // 4. Decodificar payload
  try {
    const payload = JSON.parse(atob(parts[1]));
    console.log('6. Payload:', payload);
    
    // Verificar claims importantes
    console.log('7. LoginId:', payload.LoginId || payload.loginId);
    console.log('8. TipoUsuarioId:', payload.TipoUsuarioId || payload.tipoUsuarioId);
    console.log('9. Email:', payload.email);
    console.log('10. Nome:', payload.nome);
    console.log('11. Expiração:', payload.exp ? new Date(payload.exp * 1000) : 'Não definida');
    console.log('12. Token expirado:', payload.exp ? (Date.now() / 1000 > payload.exp) : 'Não verificado');
    
  } catch (error) {
    console.log('❌ Erro ao decodificar payload:', error);
  }
  
  // 5. Testar requisição para API
  console.log('13. Testando requisição para API...');
  
  fetch('http://localhost:5062/api/login/Perfil', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  })
  .then(response => {
    console.log('14. Status da resposta:', response.status);
    console.log('15. Headers da resposta:', Object.fromEntries(response.headers.entries()));
    
    if (response.ok) {
      return response.json();
    } else {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
  })
  .then(data => {
    console.log('16. Dados do perfil:', data);
    console.log('✅ Token está funcionando corretamente!');
  })
  .catch(error => {
    console.log('❌ Erro na requisição:', error.message);
    console.log('❌ Token pode estar inválido ou expirado');
  });
}

// Função para testar login
function testLogin(usuario, senha) {
  console.log('🔍 === TESTE DE LOGIN ===');
  
  fetch('http://localhost:5062/api/login/Autenticar', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ usuario, senha })
  })
  .then(response => {
    console.log('Status:', response.status);
    return response.json();
  })
  .then(data => {
    console.log('Resposta do login:', data);
    
    if (data.token) {
      console.log('✅ Login bem-sucedido!');
      console.log('Token gerado:', data.token.substring(0, 50) + '...');
      console.log('LoginId:', data.loginId);
      console.log('TipoUsuarioId:', data.tipoUsuarioId);
    } else {
      console.log('❌ Login falhou - sem token na resposta');
    }
  })
  .catch(error => {
    console.log('❌ Erro no login:', error);
  });
}

// Função para testar o token atual
function testCurrentToken() {
  console.log('🔍 === TESTE DO TOKEN ATUAL ===');
  
  const token = localStorage.getItem('token');
  if (!token) {
    console.log('❌ Nenhum token encontrado');
    return;
  }
  
  // Testar endpoint simples
  fetch('http://localhost:5062/api/TipoPergunta', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  })
  .then(response => {
    console.log('Status:', response.status);
    if (response.ok) {
      return response.json();
    } else {
      throw new Error(`HTTP ${response.status}`);
    }
  })
  .then(data => {
    console.log('✅ Token funciona! Dados recebidos:', data);
  })
  .catch(error => {
    console.log('❌ Token não funciona:', error.message);
  });
}

// Função para limpar localStorage
function clearAuth() {
  console.log('🧹 Limpando dados de autenticação...');
  localStorage.removeItem('token');
  localStorage.removeItem('userId');
  localStorage.removeItem('tipousuarioid');
  console.log('✅ Dados limpos!');
}

// Exportar funções para uso no console
window.debugToken = debugToken;
window.testLogin = testLogin;
window.testCurrentToken = testCurrentToken;
window.clearAuth = clearAuth;

console.log('🔧 Funções de debug carregadas:');
console.log('- debugToken() - Analisa o token atual');
console.log('- testLogin(usuario, senha) - Testa login');
console.log('- testCurrentToken() - Testa token atual');
console.log('- clearAuth() - Limpa dados de auth');
