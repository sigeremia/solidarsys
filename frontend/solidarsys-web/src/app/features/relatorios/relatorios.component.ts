import {
  afterNextRender,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  OnInit,
  signal,
  viewChild,
  WritableSignal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../core/services/auth.service';
import { RelatoriosService } from './relatorios.service';
import { PeriodoRelatorio, Relatorio } from './models/relatorio.model';

// ───── Tipos de apoio para desenhar os gráficos em SVG ─────

interface Escala {
  max: number;
  marcas: number[];
}

interface Grade {
  y: number;
  rotulo: string;
}

interface RotuloX {
  x: number;
  texto: string;
}

interface Margens {
  topo: number;
  dir: number;
  base: number;
  esq: number;
}

interface BarraSvg {
  colunaX: number;
  colunaLargura: number;
  caminho: string;
  dica: string;
}

interface GraficoBarras {
  largura: number;
  altura: number;
  margens: Margens;
  grades: Grade[];
  rotulosX: RotuloX[];
  barras: BarraSvg[];
  vazio: boolean;
}

interface PontoSvg {
  x: number;
  y: number;
  dica: string;
}

interface GraficoLinha {
  largura: number;
  altura: number;
  margens: Margens;
  grades: Grade[];
  rotulosX: RotuloX[];
  caminho: string;
  pontos: PontoSvg[];
  mostrarMarcadores: boolean;
  vazio: boolean;
}

interface FatiaSvg {
  nome: string;
  cor: string;
  caminho: string;
  cheia: boolean;
  percentual: number;
  total: number;
  dica: string;
  linha: string;
  rotuloX: number;
  rotuloY: number;
  ancora: 'start' | 'end';
  rotulo: string;
}

interface GraficoPizza {
  largura: number;
  altura: number;
  cx: number;
  cy: number;
  raio: number;
  fatias: FatiaSvg[];
  rotulosExternos: boolean;
  vazio: boolean;
}

interface OpcaoPeriodo {
  label: string;
  value: PeriodoRelatorio;
}

const MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
const CORES_PIZZA = ['#14b8a6', '#f59e0b', '#3b82f6', '#22c55e', '#a855f7', '#ec4899', '#ef4444', '#64748b'];
const ALTURA_GRAFICO = 260;
const MENSAGEM_SEM_DADOS = 'Nenhum dado encontrado para este período.';

@Component({
  selector: 'app-relatorios',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, SelectModule, TooltipModule],
  templateUrl: './relatorios.component.html',
  styleUrl: './relatorios.component.css',
})
export class RelatoriosComponent implements OnInit {
  private readonly service = inject(RelatoriosService);
  private readonly destroyRef = inject(DestroyRef);
  readonly auth = inject(AuthService);

  private assinatura?: Subscription;
  private observador?: ResizeObserver;

  readonly opcoesPeriodo: OpcaoPeriodo[] = [
    { label: 'Mês atual', value: 'mensal' },
    { label: 'Últimos 3 meses', value: 'trimestral' },
    { label: 'Últimos 6 meses', value: 'semestral' },
    { label: 'Último ano', value: 'anual' },
  ];

  periodo = signal<PeriodoRelatorio>('semestral');
  dados = signal<Relatorio | null>(null);
  carregando = signal(true);
  erro = signal<string | null>(null);

  // Larguras reais (px) das áreas dos gráficos, atualizadas via ResizeObserver
  private larguraBarras = signal(0);
  private larguraLinha = signal(0);
  private larguraPizza = signal(0);

  private caixaBarras = viewChild<ElementRef<HTMLElement>>('caixaBarras');
  private caixaLinha = viewChild<ElementRef<HTMLElement>>('caixaLinha');
  private caixaPizza = viewChild<ElementRef<HTMLElement>>('caixaPizza');

  readonly mensagemSemDados = MENSAGEM_SEM_DADOS;

  constructor() {
    afterNextRender(() => this.observarLarguras());
    this.destroyRef.onDestroy(() => {
      this.assinatura?.unsubscribe();
      this.observador?.disconnect();
    });
  }

  ngOnInit(): void {
    this.carregar();
  }

  // ───── Carregamento ─────

  alterarPeriodo(novo: PeriodoRelatorio): void {
    this.periodo.set(novo);
    this.carregar();
  }

  private carregar(): void {
    this.assinatura?.unsubscribe();
    this.carregando.set(true);
    this.erro.set(null);

    this.assinatura = this.service.obter(this.periodo()).subscribe({
      next: relatorio => {
        this.dados.set(relatorio);
        this.carregando.set(false);
      },
      error: (e: HttpErrorResponse) => {
        this.carregando.set(false);

        if (e.status === 403) {
          this.dados.set(null);
          this.erro.set('Você não possui permissão para visualizar os relatórios.');
          return;
        }

        const atual = this.dados();
        if (atual) {
          // Falha ao aplicar o filtro: mantém os dados anteriores e volta o filtro ao período exibido
          this.periodo.set(atual.periodo);
          this.erro.set('Não foi possível atualizar os dados. Exibindo as informações anteriores.');
        } else {
          this.erro.set('Não foi possível carregar os dados do relatório. Tente novamente mais tarde.');
        }
      },
    });
  }

  // ───── Textos derivados ─────

  readonly semDadosNoSistema = computed(() => {
    const d = this.dados();
    return !!d && d.semDados && d.totalBeneficiarios === 0;
  });

  readonly cards = computed(() => {
    const d = this.dados();
    const vazio = '—';
    return [
      {
        rotulo: 'Total de Beneficiários',
        valor: d ? String(d.totalBeneficiarios) : vazio,
        classe: 'valor-teal',
        dica: '',
      },
      {
        rotulo: 'Total em Doações',
        valor: d ? this.moeda(d.totalDoacoes) : vazio,
        classe: 'valor-laranja',
        dica: d ? `De ${this.dataBr(d.inicio)} a ${this.dataBr(d.fim)}` : '',
      },
      {
        rotulo: 'Comunidades Atendidas',
        valor: d ? String(d.comunidadesAtendidas) : vazio,
        classe: 'valor-azul',
        dica: '',
      },
    ];
  });

  readonly tituloBarras = computed(() =>
    this.dados()?.granularidade === 'DIA' ? 'Beneficiários Cadastrados por Dia' : 'Beneficiários Cadastrados por Mês',
  );

  // ───── Gráfico de barras: beneficiários cadastrados ─────

  readonly barras = computed<GraficoBarras | null>(() => {
    const d = this.dados();
    const largura = this.larguraBarras();
    if (!d || largura < 120) return null;

    const margens: Margens = { topo: 12, dir: 12, base: 30, esq: 40 };
    const areaW = largura - margens.esq - margens.dir;
    const areaH = ALTURA_GRAFICO - margens.topo - margens.base;
    const valores = d.serie.map(p => p.beneficiarios);
    const escala = this.criarEscala(Math.max(0, ...valores), true);
    const passoX = areaW / Math.max(1, d.serie.length);
    const larguraBarra = Math.min(passoX * 0.62, 64);
    const rotulos = this.rotulosDaSerie(d);

    const barras = d.serie.map((p, i): BarraSvg => {
      const altura = (p.beneficiarios / escala.max) * areaH;
      const x = margens.esq + passoX * i + (passoX - larguraBarra) / 2;
      const y = margens.topo + areaH - altura;
      return {
        colunaX: margens.esq + passoX * i,
        colunaLargura: passoX,
        caminho: altura > 0 ? this.caminhoBarra(x, y, larguraBarra, altura) : '',
        dica: `${rotulos.longos[i]}: ${p.beneficiarios} beneficiário(s) cadastrado(s)`,
      };
    });

    const salto = this.saltoDeRotulos(rotulos.curtos, passoX);
    const rotulosX = d.serie
      .map((_, i): RotuloX => ({ x: margens.esq + passoX * i + passoX / 2, texto: rotulos.curtos[i] }))
      .filter((_, i) => i % salto === 0);

    return {
      largura,
      altura: ALTURA_GRAFICO,
      margens,
      grades: this.criarGrades(escala, margens, areaH, v => String(v)),
      rotulosX,
      barras,
      vazio: valores.every(v => v === 0),
    };
  });

  // ───── Gráfico de linha: volume de doações ─────

  readonly linha = computed<GraficoLinha | null>(() => {
    const d = this.dados();
    const largura = this.larguraLinha();
    if (!d || largura < 120) return null;

    const valores = d.serie.map(p => p.doacoes);
    const escala = this.criarEscala(Math.max(0, ...valores), false);
    const formatarMarca = (v: number) => v.toLocaleString('pt-BR');
    const maiorRotulo = Math.max(...escala.marcas.map(m => formatarMarca(m).length));
    const margens: Margens = { topo: 12, dir: 16, base: 30, esq: Math.max(40, 16 + maiorRotulo * 6.6) };
    const areaW = largura - margens.esq - margens.dir;
    const areaH = ALTURA_GRAFICO - margens.topo - margens.base;
    const rotulos = this.rotulosDaSerie(d);
    const n = d.serie.length;
    const folga = 16;

    const pontos = d.serie.map((p, i): PontoSvg => ({
      x: n === 1 ? margens.esq + areaW / 2 : margens.esq + folga + ((areaW - folga * 2) * i) / (n - 1),
      y: margens.topo + areaH - (p.doacoes / escala.max) * areaH,
      dica: `${rotulos.longos[i]}: ${this.moeda(p.doacoes)}`,
    }));

    // Curva suave sem overshoot (nunca passa abaixo do eixo)
    const f = (v: number) => v.toFixed(1);
    const caminho = pontos
      .map((p, i) => {
        if (i === 0) return `M${f(p.x)},${f(p.y)}`;
        const ant = pontos[i - 1];
        const meio = (ant.x + p.x) / 2;
        return `C${f(meio)},${f(ant.y)} ${f(meio)},${f(p.y)} ${f(p.x)},${f(p.y)}`;
      })
      .join(' ');

    const passoX = n > 1 ? (areaW - folga * 2) / (n - 1) : areaW;
    const salto = this.saltoDeRotulos(rotulos.curtos, passoX);
    const rotulosX = pontos
      .map((p, i): RotuloX => ({ x: p.x, texto: rotulos.curtos[i] }))
      .filter((_, i) => i % salto === 0);

    return {
      largura,
      altura: ALTURA_GRAFICO,
      margens,
      grades: this.criarGrades(escala, margens, areaH, formatarMarca),
      rotulosX,
      caminho,
      pontos,
      mostrarMarcadores: n <= 14,
      vazio: valores.every(v => v === 0),
    };
  });

  // ───── Gráfico de pizza: distribuição por comunidade ─────

  readonly pizza = computed<GraficoPizza | null>(() => {
    const d = this.dados();
    const largura = this.larguraPizza();
    if (!d || largura < 120) return null;

    const itens = d.comunidades.filter(c => c.totalBeneficiarios > 0);
    const total = itens.reduce((soma, c) => soma + c.totalBeneficiarios, 0);
    const rotulosExternos = largura >= 520 && itens.length <= 8;
    const altura = rotulosExternos ? 280 : 240;
    const raio = Math.min(100, largura / 2 - 16);
    const cx = largura / 2;
    const cy = altura / 2;
    const f = (v: number) => v.toFixed(2);

    const percentuais = this.percentuaisInteiros(itens.map(c => c.totalBeneficiarios));

    let angulo = -Math.PI / 2;
    const fatias = itens.map((c, i): FatiaSvg => {
      const fracao = c.totalBeneficiarios / total;
      const inicio = angulo;
      const fim = angulo + fracao * Math.PI * 2;
      angulo = fim;
      const meio = (inicio + fim) / 2;
      const percentual = percentuais[i];
      const cheia = fracao >= 0.9999;
      const cor = CORES_PIZZA[i % CORES_PIZZA.length];

      const x0 = cx + raio * Math.cos(inicio);
      const y0 = cy + raio * Math.sin(inicio);
      const x1 = cx + raio * Math.cos(fim);
      const y1 = cy + raio * Math.sin(fim);
      const xa = cx + (raio + 4) * Math.cos(meio);
      const ya = cy + (raio + 4) * Math.sin(meio);
      const xb = cx + (raio + 18) * Math.cos(meio);
      const yb = cy + (raio + 18) * Math.sin(meio);
      const direita = Math.cos(meio) >= 0;

      return {
        nome: c.nome,
        cor,
        cheia,
        percentual,
        total: c.totalBeneficiarios,
        caminho: cheia ? '' : `M${f(cx)},${f(cy)} L${f(x0)},${f(y0)} A${raio},${raio} 0 ${fracao > 0.5 ? 1 : 0} 1 ${f(x1)},${f(y1)} Z`,
        dica: `${c.nome}: ${c.totalBeneficiarios} beneficiário(s) (${percentual}%)`,
        linha: `${f(xa)},${f(ya)} ${f(xb)},${f(yb)}`,
        rotuloX: xb + (direita ? 6 : -6),
        rotuloY: yb + 4,
        ancora: direita ? 'start' : 'end',
        rotulo: `${this.abreviar(c.nome, 24)} (${percentual}%)`,
      };
    });

    return { largura, altura, cx, cy, raio, fatias, rotulosExternos, vazio: itens.length === 0 };
  });

  // ───── Exportação ─────

  exportarCsv(): void {
    const d = this.dados();
    if (!d) return;

    const diario = d.granularidade === 'DIA';
    const percentuais = this.percentuaisInteiros(d.comunidades.map(c => c.totalBeneficiarios));

    const linhas: string[][] = [
      ['Relatório SolidarSYS'],
      ['Período', this.rotuloPeriodo(d.periodo)],
      ['Intervalo', `${this.dataBr(d.inicio)} a ${this.dataBr(d.fim)}`],
      ['Gerado em', new Date().toLocaleString('pt-BR')],
      [],
      ['Indicadores'],
      ['Total de Beneficiários', String(d.totalBeneficiarios)],
      ['Total em Doações (R$)', this.numeroCsv(d.totalDoacoes)],
      ['Comunidades Atendidas', String(d.comunidadesAtendidas)],
      [],
      [diario ? 'Data' : 'Mês', 'Beneficiários cadastrados', 'Doações (R$)'],
      ...d.serie.map(p => [this.dataSerieCsv(p.data, diario), String(p.beneficiarios), this.numeroCsv(p.doacoes)]),
      [],
      ['Comunidade', 'Beneficiários', 'Percentual'],
      ...d.comunidades.map((c, i) => [c.nome, String(c.totalBeneficiarios), `${percentuais[i]}%`]),
    ];

    const conteudo = linhas.map(l => l.map(c => this.campoCsv(c)).join(';')).join('\r\n');
    const blob = new Blob(['\uFEFF' + conteudo], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `relatorio-solidarsys-${d.periodo}-${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
  }

  /** Abre o diálogo de impressão do navegador, onde basta escolher "Salvar como PDF". */
  exportarPdf(): void {
    const d = this.dados();
    if (!d) return;

    const tituloOriginal = document.title;
    document.title = `Relatorio-SolidarSYS-${d.periodo}-${new Date().toISOString().slice(0, 10)}`;
    document.body.classList.add('imprimindo-relatorio');

    const restaurar = () => {
      document.body.classList.remove('imprimindo-relatorio');
      document.title = tituloOriginal;
      window.removeEventListener('afterprint', restaurar);
    };
    window.addEventListener('afterprint', restaurar);

    // Pequena espera para o navegador aplicar a classe de impressão
    setTimeout(() => window.print(), 50);
  }

  geradoEm(): string {
    return new Date().toLocaleString('pt-BR');
  }

  rotuloPeriodo(periodo: PeriodoRelatorio): string {
    return this.opcoesPeriodo.find(o => o.value === periodo)?.label ?? periodo;
  }

  intervalo(d: Relatorio): string {
    return `${this.dataBr(d.inicio)} a ${this.dataBr(d.fim)}`;
  }

  // ───── Utilidades ─────

  private observarLarguras(): void {
    const alvos: [ElementRef<HTMLElement> | undefined, WritableSignal<number>][] = [
      [this.caixaBarras(), this.larguraBarras],
      [this.caixaLinha(), this.larguraLinha],
      [this.caixaPizza(), this.larguraPizza],
    ];

    if (typeof ResizeObserver === 'undefined') {
      alvos.forEach(([, sinal]) => sinal.set(600));
      return;
    }

    const mapa = new Map<Element, WritableSignal<number>>();
    this.observador = new ResizeObserver(entradas => {
      for (const entrada of entradas) {
        mapa.get(entrada.target)?.set(Math.floor(entrada.contentRect.width));
      }
    });

    for (const [ref, sinal] of alvos) {
      if (!ref) continue;
      mapa.set(ref.nativeElement, sinal);
      this.observador.observe(ref.nativeElement);
    }
  }

  /** Escala "bonita" (passos 1, 2, 2,5, 5 × 10ⁿ). Para contagens, os passos são sempre inteiros. */
  private criarEscala(maior: number, inteiro: boolean, alvo = 4): Escala {
    const bruto = (maior > 0 ? maior : alvo) / alvo;
    const magnitude = Math.pow(10, Math.floor(Math.log10(bruto)));
    const normal = bruto / magnitude;
    const passoNormal = normal <= 1 ? 1 : normal <= 2 ? 2 : normal <= 2.5 ? 2.5 : normal <= 5 ? 5 : 10;
    let passo = passoNormal * magnitude;
    if (inteiro) passo = Math.max(1, Math.ceil(passo));

    let max = Math.ceil(maior / passo) * passo;
    if (max <= 0) max = passo * alvo;

    const quantidade = Math.round(max / passo);
    const marcas = Array.from({ length: quantidade + 1 }, (_, i) => Math.round(i * passo * 1e6) / 1e6);
    return { max, marcas };
  }

  private criarGrades(escala: Escala, m: Margens, areaH: number, formatar: (v: number) => string): Grade[] {
    return escala.marcas.map(v => ({
      y: m.topo + areaH - (v / escala.max) * areaH,
      rotulo: formatar(v),
    }));
  }

  /** Retângulo com cantos superiores arredondados. */
  private caminhoBarra(x: number, y: number, largura: number, altura: number): string {
    const r = Math.min(4, largura / 2, altura);
    const f = (v: number) => v.toFixed(1);
    return `M${f(x)},${f(y + altura)} V${f(y + r)} Q${f(x)},${f(y)} ${f(x + r)},${f(y)} H${f(x + largura - r)} Q${f(x + largura)},${f(y)} ${f(x + largura)},${f(y + r)} V${f(y + altura)} Z`;
  }

  private rotulosDaSerie(d: Relatorio): { curtos: string[]; longos: string[] } {
    const diario = d.granularidade === 'DIA';
    const anos = d.serie.map(p => Number(p.data.slice(0, 4)));
    const cruzaAno = anos.length > 0 && anos[0] !== anos[anos.length - 1];

    const curtos = d.serie.map(p => {
      const [ano, mes, dia] = p.data.split('-').map(Number);
      if (diario) return `${String(dia).padStart(2, '0')}/${String(mes).padStart(2, '0')}`;
      return cruzaAno ? `${MESES[mes - 1]}/${String(ano).slice(2)}` : MESES[mes - 1];
    });

    const longos = d.serie.map(p => {
      const [ano, mes, dia] = p.data.split('-').map(Number);
      if (diario) return `${String(dia).padStart(2, '0')}/${String(mes).padStart(2, '0')}/${ano}`;
      return `${MESES[mes - 1]}/${ano}`;
    });

    return { curtos, longos };
  }

  /** De quantos em quantos pontos mostrar o rótulo do eixo X para que os textos não se sobreponham. */
  private saltoDeRotulos(rotulos: string[], passoX: number): number {
    const maior = Math.max(...rotulos.map(t => t.length));
    return Math.max(1, Math.ceil((maior * 6.2 + 8) / Math.max(1, passoX)));
  }

  /** Percentuais inteiros que somam 100 (método do maior resto). */
  private percentuaisInteiros(valores: number[]): number[] {
    const total = valores.reduce((soma, v) => soma + v, 0);
    if (total <= 0) return valores.map(() => 0);

    const exatos = valores.map(v => (v / total) * 100);
    const base = exatos.map(Math.floor);
    let restante = 100 - base.reduce((soma, v) => soma + v, 0);

    const porResto = exatos
      .map((v, i) => ({ i, resto: v - base[i] }))
      .sort((a, b) => b.resto - a.resto);
    for (const { i } of porResto) {
      if (restante <= 0) break;
      base[i] += 1;
      restante -= 1;
    }
    return base;
  }

  private abreviar(texto: string, limite: number): string {
    return texto.length > limite ? `${texto.slice(0, limite - 1)}…` : texto;
  }

  private moeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      minimumFractionDigits: Number.isInteger(valor) ? 0 : 2,
      maximumFractionDigits: 2,
    }).format(valor);
  }

  private dataBr(iso: string): string {
    const [ano, mes, dia] = iso.split('-');
    return `${dia}/${mes}/${ano}`;
  }

  private dataSerieCsv(iso: string, diario: boolean): string {
    const [ano, mes, dia] = iso.split('-');
    return diario ? `${dia}/${mes}/${ano}` : `${mes}/${ano}`;
  }

  private numeroCsv(valor: number): string {
    return valor.toFixed(2).replace('.', ',');
  }

  /** Escapa o campo e neutraliza fórmulas (=, +, -, @) para planilhas. */
  private campoCsv(valor: string): string {
    let texto = valor ?? '';
    if (/^[=+\-@\t\r]/.test(texto) && !/^-?\d+([.,]\d+)?%?$/.test(texto)) texto = `'${texto}`;
    return /[;"\r\n]/.test(texto) ? `"${texto.replace(/"/g, '""')}"` : texto;
  }
}
