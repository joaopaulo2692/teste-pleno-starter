import React, { useEffect, useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiGet, apiPost, apiPut, apiDelete } from '../api'

export default function VeiculosPage() {
  const qc = useQueryClient()
  const [clienteId, setClienteId] = useState('')
  const clientes = useQuery({ queryKey: ['clientes-mini'], queryFn: () => apiGet('/api/clientes?pagina=1&tamanho=100') })
  const veiculos = useQuery({ queryKey: ['veiculos', clienteId], queryFn: () => apiGet(`/api/veiculos${clienteId ? `?clienteId=${clienteId}` : ''}`) })
  const [form, setForm] = useState({ placa: '', modelo: '', ano: '', clienteId: '' })

  // const create = useMutation({
  //   mutationFn: (data) => apiPost('/api/veiculos', data),
  //   onSuccess: () => qc.invalidateQueries({ queryKey: ['veiculos'] })
  // })
  const create = useMutation({
  mutationFn: (data) => apiPost('/api/veiculos', data),
  onSuccess: (data) => {
    qc.invalidateQueries({ queryKey: ['veiculos'] })
    alert('Veículo criado com sucesso!')
    setForm({ placa: '', modelo: '', ano: '', clienteId: clienteId }) // limpa formulário
  },
  onError: (err) => {
    // Se a API retornar { message: "..." } ou apenas string
    const msg = err?.response?.data?.message || err?.message || 'Erro ao criar veículo.'
    alert(msg)
  }
})

  // const update = useMutation({
  //   mutationFn: ({ id, data }) => apiPut(`/api/veiculos/${id}`, data),
  //   onSuccess: () => qc.invalidateQueries({ queryKey: ['veiculos'] })
  // })

  const update = useMutation({
  mutationFn: ({ id, data }) => apiPut(`/api/veiculos/${id}`, data),
  onSuccess: (data) => {
    qc.invalidateQueries({ queryKey: ['veiculos'] })
    alert('Veículo atualizado com sucesso!')
  },
  onError: (err) => {
    const msg = err?.response?.data?.message || err?.message || 'Erro ao atualizar veículo.'
    alert(msg)
  }
})

  const remover = useMutation({
    mutationFn: (id) => apiDelete(`/api/veiculos/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['veiculos'] })
  })

  const [editVeiculo, setEditVeiculo] = useState(null) // { id, placa, modelo, ano, clienteId }

  useEffect(() => {
    if (clientes.data?.itens?.length && !clienteId) {
      setClienteId(clientes.data.itens[0].id)
      setForm(f => ({ ...f, clienteId: clientes.data.itens[0].id }))
    }
  }, [clientes.data])

  return (
    <div>
      <h2>Veículos</h2>

      <div className="section">
        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          <label>Cliente: </label>
          <select value={clienteId} onChange={e => { setClienteId(e.target.value); setForm(f => ({ ...f, clienteId: e.target.value })) }}>
            {clientes.data?.itens?.map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>
        </div>
      </div>

      <h3>Novo veículo</h3>
      <div className="section">
        <div className="grid grid-4">
          <input placeholder="Placa" value={form.placa} onChange={e => setForm({ ...form, placa: e.target.value })} />
          <input placeholder="Modelo" value={form.modelo} onChange={e => setForm({ ...form, modelo: e.target.value })} />
          <input placeholder="Ano" value={form.ano} onChange={e => setForm({ ...form, ano: e.target.value })} />
          <select value={form.clienteId || clienteId} onChange={e => setForm({ ...form, clienteId: e.target.value })}>
            {clientes.data?.itens?.map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>
          <button onClick={() => create.mutate({
            placa: form.placa, modelo: form.modelo, ano: form.ano ? Number(form.ano) : null, clienteId: form.clienteId || clienteId
          })}>Salvar</button>
        </div>
      </div>

      <h3 style={{ marginTop: 16 }}>Lista</h3>
      <div className="section">
        {veiculos.isLoading ? <p>Carregando...</p> : (
          <table>
            <thead><tr><th>Placa</th><th>Modelo</th><th>Ano</th><th>Cliente</th><th>Ações</th></tr></thead>
            <tbody>
              {veiculos.data?.map(v => (
                <tr key={v.id}>
                  <td>{v.placa}</td>
                  <td>{v.modelo}</td>
                  <td>{v.ano ?? '-'}</td>
                  <td>{clientes.data?.itens?.find(c => c.id === v.clienteId)?.nome || '-'}</td>
                  <td style={{ display: 'flex', gap: 8 }}>
                    <button className="btn-ghost" onClick={() => setEditVeiculo(v)}>Editar</button>
                    <button className="btn-ghost" onClick={() => remover.mutate(v.id)}>Excluir</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {editVeiculo && (
          <div className="modal">
            <h3>Editar veículo {editVeiculo.placa}</h3>
            <input
              placeholder="Modelo"
              value={editVeiculo.modelo || ''}
              onChange={e => setEditVeiculo({ ...editVeiculo, modelo: e.target.value })}
            />
            <input
              type="number"
              placeholder="Ano"
              value={editVeiculo.ano || ''}
              onChange={e => setEditVeiculo({ ...editVeiculo, ano: Number(e.target.value) })}
            />
            <select
              value={editVeiculo.clienteId}
              onChange={e => setEditVeiculo({ ...editVeiculo, clienteId: e.target.value })}
            >
              {clientes.data?.itens?.map(c => (
                <option key={c.id} value={c.id}>{c.nome}</option>
              ))}
            </select>
            <button onClick={() => { update.mutate({ id: editVeiculo.id, data: editVeiculo }); setEditVeiculo(null) }}>Salvar</button>
            <button onClick={() => setEditVeiculo(null)}>Cancelar</button>
          </div>
        )}
      </div>
    </div>
  )
}
