import React, { useState } from 'react'

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'

import { apiGet, apiPost, apiDelete, apiPut } from '../api'

// Função auxiliar para extrair a mensagem de erro do objeto de erro
const getErrorMessage = (error) => {
    // Tenta extrair a mensagem de erro do backend se for um erro de resposta HTTP (ex: usando axios)
    if (error.response && error.response.data && error.response.data.message) {
        return error.response.data.message
    }
    // Caso contrário, usa a mensagem de erro padrão
    return error instanceof Error ? error.message : 'Ocorreu um erro desconhecido.'
}

export default function ClientesPage() {

    const qc = useQueryClient()

    const [filtro, setFiltro] = useState('')

    const [mensalista, setMensalista] = useState('all')

    const [form, setForm] = useState({ id: null, nome: '', telefone: '', endereco: '', mensalista: false, valorMensalidade: '' })

    const [erro, setErro] = useState('')

    const [sucesso, setSucesso] = useState('')

    const [editando, setEditando] = useState(false)

    const q = useQuery({

        queryKey: ['clientes', filtro, mensalista],

        queryFn: () =>
            apiGet(
                `/api/clientes?pagina=1&tamanho=20&filtro=${encodeURIComponent(filtro)}&mensalista=${mensalista}`
            )

    })

    const create = useMutation({

        mutationFn: (data) => apiPost('/api/clientes', data),

        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['clientes'] })
            limparForm()
            setErro('')
            setSucesso('Cliente criado com sucesso!')
            setTimeout(() => setSucesso(''), 3000)
        },

        onError: (error) => {
            // Usa a função auxiliar para extrair a mensagem de erro
            setErro(getErrorMessage(error))
            setSucesso('')
        }
    })

    const update = useMutation({

        mutationFn: (data) => apiPut(`/api/clientes/${data.id}`, data),

        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['clientes'] })
            limparForm()
            setEditando(false)
            setErro('')
            setSucesso('Cliente atualizado com sucesso!')
            setTimeout(() => setSucesso(''), 3000)
        },

        onError: (error) => {
            // Usa a função auxiliar para extrair a mensagem de erro
            setErro(getErrorMessage(error))
            setSucesso('')
        }
    })

    const remover = useMutation({

        mutationFn: (id) => apiDelete(`/api/clientes/${id}`),

        onSuccess: () => {
            qc.invalidateQueries({ queryKey: ['clientes'] })
            setErro('')
            setSucesso('Cliente excluído com sucesso!') // Mensagem de sucesso para a exclusão
            setTimeout(() => setSucesso(''), 3000)
        },
        
        onError: (error) => {
            // Usa a função auxiliar para extrair a mensagem de erro
            setErro(getErrorMessage(error))
            setSucesso('')
        }
    })

    function limparForm() {

        setForm({ id: null, nome: '', telefone: '', endereco: '', mensalista: false, valorMensalidade: '' })

    }

    function salvar() {

        // Limpa mensagens de erro e sucesso antes de iniciar a mutação
        setErro('')
        setSucesso('')
        
        const data = {

            nome: form.nome,

            telefone: form.telefone,

            endereco: form.endereco,

            mensalista: form.mensalista,

            valorMensalidade: form.valorMensalidade ? Number(form.valorMensalidade) : null

        }
        if (editando && form.id) update.mutate({ id: form.id, ...data })
        else create.mutate(data)

    }

    // Função para tratar a exclusão e limpar mensagens
    function handleRemover(id) {
        // Limpa mensagens de erro e sucesso antes de iniciar a mutação
        setErro('')
        setSucesso('')
        remover.mutate(id)
    }

    return (
        <div>
            <h2>Clientes</h2>
            <div className="section">
                <div className="grid grid-3">
                    <input placeholder="Buscar por nome" value={filtro} onChange={e => setFiltro(e.target.value)} />
                    <select value={mensalista} onChange={e => setMensalista(e.target.value)}>
                        <option value="all">Todos</option>
                        <option value="true">Mensalistas</option>
                        <option value="false">Não mensalistas</option>
                    </select>
                    <div />
                </div>
            </div>

            <h3>{editando ? 'Editar cliente' : 'Novo cliente'}</h3>
            <div className="section">
                <div className="grid grid-4">
                    <input placeholder="Nome" value={form.nome} onChange={e => setForm({ ...form, nome: e.target.value })} />
                    <input placeholder="Telefone" value={form.telefone} onChange={e => setForm({ ...form, telefone: e.target.value })} />
                    <input placeholder="Endereço" value={form.endereco} onChange={e => setForm({ ...form, endereco: e.target.value })} />
                    <label style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                        <input type="checkbox" checked={form.mensalista} onChange={e => setForm({ ...form, mensalista: e.target.checked })} /> Mensalista
                    </label>
                    <input placeholder="Valor mensalidade" value={form.valorMensalidade} onChange={e => setForm({ ...form, valorMensalidade: e.target.value })} />
                    <div />
                    <div />
                    <button onClick={salvar}>{editando ? 'Salvar alterações' : 'Salvar'}</button>
                    {editando && (
                        <button style={{ marginLeft: 8 }} onClick={() => { limparForm(); setEditando(false); setErro(''); setSucesso(''); }}>
                            Cancelar
                        </button>
                    )}
                </div>

                {/* Exibe mensagens de erro ou sucesso */}
                {erro && <p style={{ color: 'red' }}>Erro: {erro}</p>}
                {sucesso && <p style={{ color: 'green' }}>Sucesso: {sucesso}</p>}
            </div>

            <h3 style={{ marginTop: 16 }}>Lista</h3>
            <div className="section">
                {q.isLoading ? (
                    <p>Carregando...</p>
                ) : q.isError ? ( // Adicionando tratamento de erro para a query de listagem
                    <p style={{ color: 'red' }}>Erro ao carregar clientes: {getErrorMessage(q.error)}</p>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>Nome</th>
                                <th>Telefone</th>
                                <th>Mensalista</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {q.data.itens.map(c => (
                                <tr key={c.id}>
                                    <td>{c.nome}</td>
                                    <td>{c.telefone}</td>
                                    <td>{c.mensalista ? 'Sim' : 'Não'}</td>
                                    <td>
                                        <button className="btn-ghost" onClick={() => {
                                            setForm({
                                                id: c.id,
                                                nome: c.nome,
                                                telefone: c.telefone,
                                                endereco: c.endereco,
                                                mensalista: c.mensalista,
                                                valorMensalidade: c.valorMensalidade || ''
                                            })
                                            setEditando(true)
                                            setErro('')
                                            setSucesso('')
                                        }}>Editar</button>

                                        <button className="btn-ghost" onClick={() => handleRemover(c.id)}>Excluir</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
        </div>

    )

}