import React, { useState } from 'react'

// Função simples que respeita aspas no CSV
function parseCsvLine(line) {
  const cols = []
  let current = ''
  let inQuotes = false

  for (let i = 0; i < line.length; i++) {
    const char = line[i]

    if (char === '"') {
      inQuotes = !inQuotes
    } else if (char === ',' && !inQuotes) {
      cols.push(current.trim())
      current = ''
    } else {
      current += char
    }
  }
  cols.push(current.trim())
  return cols
}

export default function CsvUploadPage() {
  const [log, setLog] = useState(null)

  async function handleUpload(e) {
    e.preventDefault()
    const file = e.target.file.files[0]
    const fd = new FormData()
    fd.append('file', file)
    const r = await fetch(
      (import.meta.env.VITE_API_URL || 'https://localhost:57003') + '/api/import/csv',
      { method: 'POST', body: fd }
    )
    const j = await r.json()
    setLog(j)
  }

  return (
    <div>
      <h2>Importar CSV</h2>
      <div className="section">
        <form
          onSubmit={handleUpload}
          style={{ display: 'flex', gap: 10, alignItems: 'center' }}
        >
          <input type="file" name="file" accept=".csv" />
          <button type="submit">Enviar</button>
        </form>
      </div>

      <h3 style={{ marginTop: 16 }}>Relatório</h3>
      <div className="section">
        {!log && <p>Aguardando upload...</p>}
        {log && (
          <table
            style={{
              borderCollapse: 'collapse',
              width: '100%',
              marginTop: 10,
            }}
          >
            <thead>
              <tr>
                {['Linha','Placa','Modelo','Ano','CLI','Nome','Telefone','Endereço','Mensalista','Mensalidade','Motivo'].map((h, i) => (
                  <th key={i} style={{ border: '1px solid #ccc', padding: 6, textAlign: 'left' }}>
                    {h}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {log.erros.map((raw, idx) => {
                const m = raw.match(/Linha (\d+): (.+) \(raw='(.+)'\)/)
                if (!m) return null
                const linha = m[1]
                const motivo = m[2]
                const rawCsv = m[3]

                const cols = parseCsvLine(rawCsv)

                // Mensalista Sim/Não
                if (cols.length >= 8) cols[7] = cols[7].toLowerCase() === 'true' ? 'Sim' : 'Não'

                // Mensalidade
                if (cols.length >= 9 && !cols[8]) cols[8] = '-'

                // Ano deve ser separado corretamente (no CSV, era confundido com CLI)
                while (cols.length < 9) cols.push('-')

                return (
                  <tr key={idx}>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{linha}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[0]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[1]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[2]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[3]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[4]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[5]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[6]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[7]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{cols[8]}</td>
                    <td style={{ border: '1px solid #ccc', padding: 6 }}>{motivo}</td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
