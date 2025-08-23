// Funções para exportação de dados em diferentes formatos

export function exportToCSV(data, filename = 'export.csv') {
    if (!Array.isArray(data) || data.length === 0) {
        console.warn('Dados inválidos para exportação CSV');
        return;
    }

    const headers = Object.keys(data[0]);
    const csvContent = [
        headers.join(','),
        ...data.map(row => 
            headers.map(header => {
                const value = row[header];
                return typeof value === 'string' && value.includes(',') 
                    ? `"${value}"` 
                    : value;
            }).join(',')
        )
    ].join('\n');

    downloadFile(csvContent, filename, 'text/csv');
}

export function exportToJSON(data, filename = 'export.json') {
    const jsonContent = JSON.stringify(data, null, 2);
    downloadFile(jsonContent, filename, 'application/json');
}

export function exportToExcel(data, filename = 'export.xlsx') {
    // Esta função requer uma biblioteca como xlsx
    // Por enquanto, vamos criar um CSV que pode ser aberto no Excel
    exportToCSV(data, filename.replace('.xlsx', '.csv'));
}

export function downloadFile(content, filename, mimeType) {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}

export function generateQRCode(text, size = 200) {
    // Esta função requer uma biblioteca como qrcode
    // Por enquanto, retorna uma URL para um serviço online
    const encodedText = encodeURIComponent(text);
    return `https://api.qrserver.com/v1/create-qr-code/?size=${size}x${size}&data=${encodedText}`;
}

export function copyToClipboard(text) {
    if (navigator.clipboard) {
        return navigator.clipboard.writeText(text);
    } else {
        // Fallback para navegadores mais antigos
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            return Promise.resolve();
        } catch (err) {
            return Promise.reject(err);
        } finally {
            document.body.removeChild(textArea);
        }
    }
}

export function shareData(data, title = 'Compartilhar dados') {
    if (navigator.share) {
        return navigator.share({
            title,
            text: JSON.stringify(data),
            url: window.location.href
        });
    } else {
        // Fallback: copiar para clipboard
        return copyToClipboard(JSON.stringify(data, null, 2));
    }
}

export function printElement(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.warn(`Elemento com ID ${elementId} não encontrado`);
        return;
    }

    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title>Impressão</title>
                <style>
                    body { font-family: Arial, sans-serif; }
                    @media print {
                        .no-print { display: none !important; }
                    }
                </style>
            </head>
            <body>
                ${element.outerHTML}
            </body>
        </html>
    `);
    printWindow.document.close();
    printWindow.print();
}
