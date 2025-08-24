// Script de teste para verificar Google Login
// Execute este script no console do navegador

console.log("🧪 TESTE GOOGLE LOGIN - FastSurvey");

// Verificar variáveis de ambiente
console.log("1. Verificando variáveis de ambiente:");
console.log("   REACT_APP_GOOGLE_CLIENT_ID:", process.env.REACT_APP_GOOGLE_CLIENT_ID);
console.log("   REACT_APP_API_URL:", process.env.REACT_APP_API_URL);

// Verificar se o script do Google foi carregado
console.log("2. Verificando script do Google:");
console.log("   Script element:", document.getElementById("google-gis"));
console.log("   window.google:", !!window.google);
console.log("   window.google.accounts:", !!window.google?.accounts);

// Verificar elementos do DOM
console.log("3. Verificando elementos do DOM:");
const googleBtnSlot = document.querySelector('[class*="googleBtnSlot"]');
console.log("   googleBtnSlot element:", googleBtnSlot);
console.log("   googleBtnSlot innerHTML:", googleBtnSlot?.innerHTML);

// Verificar se há erros no console
console.log("4. Verificando erros:");
// Os erros aparecerão automaticamente no console

// Teste de inicialização manual (se window.google existir)
if (window.google?.accounts?.id) {
  console.log("5. Testando inicialização manual:");
  try {
    window.google.accounts.id.initialize({
      client_id: process.env.REACT_APP_GOOGLE_CLIENT_ID,
      callback: (response) => console.log("Google callback:", response),
      auto_select: false,
      cancel_on_tap_outside: true,
    });
    console.log("   ✅ Inicialização manual bem-sucedida");
  } catch (error) {
    console.error("   ❌ Erro na inicialização manual:", error);
  }
} else {
  console.log("5. window.google não disponível para teste manual");
}

console.log("🧪 FIM DO TESTE");
