import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin, catchError, of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { BeneficiariosService } from '../beneficiarios/beneficiarios.service';
import { DoadoresService } from '../doadores/doadores.service';
import { VisitasService } from '../visitas/visitas.service';
import { ComunidadesService } from '../comunidades/comunidades.service';

interface MonthlyData {
  label: string;
  beneficiarios: number;
  doacoes: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard">
      <header class="dashboard-header">
        <h1>Painel de Controle</h1>
        <p>Visão geral de {{ nomeSistema }}</p>
      </header>

      @if (erroCarregamento) {
        <div class="error-banner">
          Não foi possível atualizar alguns dados do painel.
        </div>
      }

      <section class="summary-grid" aria-label="Resumo geral">
        @for (card of cards; track card.label) {
          <article class="summary-card">
            <div>
              <p class="summary-label">{{ card.label }}</p>
              <p class="summary-value">{{ carregando ? '—' : card.value }}</p>
            </div>
            <div [class]="'summary-icon ' + card.iconClass">
              <i [class]="'pi ' + card.icon"></i>
            </div>
          </article>
        }
      </section>

      <section class="charts-grid">
        <article class="chart-card evolution-card">
          <h2>Evolução Mensal</h2>
          <div class="bar-chart" aria-label="Gráfico mensal de beneficiários e doadores">
            @for (item of evolucaoMensal; track item.label) {
              <div class="bar-group">
                <div class="bars">
                  <div class="bar bar-primary" [style.height.%]="alturaBarra(item.beneficiarios)">
                    <span class="bar-tooltip">
                      <strong>{{ item.label }}</strong>
                      <span>Beneficiários: {{ item.beneficiarios }}</span>
                      <span>Doadores: {{ item.doacoes }}</span>
                    </span>
                  </div>
                  <div class="bar bar-secondary" [style.height.%]="alturaBarra(item.doacoes)">
                    <span class="bar-tooltip">
                      <strong>{{ item.label }}</strong>
                      <span>Beneficiários: {{ item.beneficiarios }}</span>
                      <span>Doadores: {{ item.doacoes }}</span>
                    </span>
                  </div>
                </div>
                <span>{{ item.label }}</span>
              </div>
            }
          </div>
        </article>

        <article class="chart-card communities-card">
          <h2>Por Comunidade</h2>
          <div class="donut-wrap">
            <div class="donut" [style.background]="donutGradient">
              <div class="donut-center">
                <strong>{{ totalComunidades }}</strong>
                <span>comunidades</span>
              </div>
            </div>
          </div>
          <div class="community-list">
            @for (comunidade of comunidades; track comunidade.nome; let i = $index) {
              <span [class]="'community-dot dot-' + (i % 4)">
                {{ comunidade.nome }}
              </span>
            }
          </div>
        </article>
      </section>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .dashboard { min-height: 100%; padding: 24px 28px 32px; background: #f8fafc; }
    .dashboard-header { margin-bottom: 24px; }
    .dashboard-header h1 { margin: 0 0 4px; color: #1e293b; font-size: 24px; font-weight: 800; }
    .dashboard-header p { margin: 0; color: #64748b; font-size: 14px; }
    .error-banner { margin-bottom: 16px; border: 1px solid #fecaca; border-radius: 10px; background: #fef2f2; color: #b91c1c; padding: 10px 12px; font-size: 13px; }
    .summary-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 20px; margin-bottom: 24px; }
    .summary-card { position: relative; min-height: 128px; display: flex; align-items: flex-start; justify-content: space-between; padding: 22px 20px; background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; box-shadow: 0 2px 8px rgba(15,23,42,.05); }
    .summary-label { margin: 0 0 12px; color: #64748b; font-size: 14px; }
    .summary-value { margin: 0; color: #1e293b; font-size: 24px; line-height: 1; font-weight: 800; }
    .summary-icon { width: 44px; height: 44px; display: grid; place-items: center; border-radius: 10px; }
    .summary-icon i { font-size: 18px; }
    .icon-teal { background: #ccfbf1; color: #0d9488; }
    .icon-orange { background: #ffedd5; color: #f97316; }
    .icon-blue { background: #dbeafe; color: #2563eb; }
    .icon-green { background: #dcfce7; color: #16a34a; }
    .charts-grid { display: grid; grid-template-columns: minmax(0, 2fr) minmax(280px, 1fr); gap: 20px; }
    .chart-card { min-height: 360px; background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; box-shadow: 0 2px 8px rgba(15,23,42,.05); padding: 20px; }
    .chart-card h2 { margin: 0; color: #1e293b; font-size: 16px; }
    .bar-chart { height: 290px; display: flex; align-items: flex-end; gap: 18px; padding: 30px 12px 0 38px; border-left: 1px solid #cbd5e1; border-bottom: 1px solid #cbd5e1; margin: 20px 6px 0 12px; background: repeating-linear-gradient(to bottom, transparent 0, transparent 52px, #eef2f7 53px); }
    .bar-group { height: 100%; flex: 1; min-width: 35px; display: flex; flex-direction: column; justify-content: flex-end; align-items: center; }
    .bars { width: 100%; height: 230px; display: flex; align-items: flex-end; justify-content: center; gap: 5px; }
    .bar { position: relative; width: 42%; min-height: 3px; border-radius: 5px 5px 0 0; }
    .bar-tooltip { display: none; position: absolute; z-index: 20; left: 50%; bottom: calc(100% + 8px); width: 150px; transform: translateX(-50%); padding: 12px; border: 1px solid #e2e8f0; border-radius: 8px; background: #fff; box-shadow: 0 8px 24px rgba(15,23,42,.16); color: #64748b; font-size: 12px; line-height: 1.8; pointer-events: none; }
    .bar-tooltip strong, .bar-tooltip span { display: block; }
    .bar-tooltip strong { color: #1e293b; font-size: 13px; }
    .bar-tooltip span:nth-child(2) { color: #14b8a6; }
    .bar-tooltip span:nth-child(3) { color: #f59e0b; }
    .bar:hover .bar-tooltip { display: block; }
    .bar-primary { background: #14b8a6; }
    .bar-secondary { background: #f59e0b; }
    .bar-group > span { height: 24px; margin-top: 8px; color: #64748b; font-size: 11px; }
    .donut-wrap { display: grid; place-items: center; min-height: 240px; }
    .donut { width: 200px; height: 200px; display: grid; place-items: center; border-radius: 50%; }
    .donut-center { width: 118px; height: 118px; display: grid; place-content: center; text-align: center; border-radius: 50%; background: #fff; }
    .donut-center strong { color: #1e293b; font-size: 24px; }
    .donut-center span { color: #64748b; font-size: 11px; }
    .community-list { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px 12px; padding-top: 4px; }
    .community-list span { position: relative; padding-left: 12px; color: #64748b; font-size: 11px; }
    .community-dot::before { content: ''; position: absolute; left: 0; top: 4px; width: 7px; height: 7px; border-radius: 50%; background: #14b8a6; }
    .dot-1::before { background: #3b82f6; }
    .dot-2::before { background: #f59e0b; }
    .dot-3::before { background: #22c55e; }
    @media (max-width: 1024px) {
      .summary-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
      .charts-grid { grid-template-columns: 1fr; }
    }
    @media (max-width: 640px) {
      .dashboard { padding: 20px 16px; }
      .summary-grid { grid-template-columns: 1fr; }
      .chart-card { min-height: 320px; }
      .bar-chart { gap: 6px; padding-left: 18px; }
    }
  `]
})
export class DashboardComponent implements OnInit {
  auth = inject(AuthService);
  private beneficiariosService = inject(BeneficiariosService);
  private doadoresService = inject(DoadoresService);
  private visitasService = inject(VisitasService);
  private comunidadesService = inject(ComunidadesService);
  private changeDetectorRef = inject(ChangeDetectorRef);

  nomeSistema = 'SolidarSYS';
  carregando = true;
  erroCarregamento = false;
  totalComunidades = 0;
  comunidades: { nome: string; totalBeneficiarios: number }[] = [];
  evolucaoMensal: MonthlyData[] = [];
  donutGradient = 'conic-gradient(#14b8a6 0 100%)';

  cards = [
    { label: 'Beneficiários', value: 0, icon: 'pi-heart', iconClass: 'icon-teal' },
    { label: 'Doadores', value: 0, icon: 'pi-users', iconClass: 'icon-orange' },
    { label: 'Visitas este mês', value: 0, icon: 'pi-calendar', iconClass: 'icon-blue' },
    { label: 'Comunidades', value: 0, icon: 'pi-map-marker', iconClass: 'icon-green' },
  ];

  ngOnInit(): void {
    forkJoin({
      beneficiarios: this.beneficiariosService.listar(1, 100).pipe(catchError(() => { this.erroCarregamento = true; return of({ total: 0, items: [] } as { total: number; items: { dataCriacao: string }[] }); })),
      doadores: this.doadoresService.listar(1, 100).pipe(catchError(() => { this.erroCarregamento = true; return of({ total: 0, items: [] } as { total: number; items: { dataCadastro: string }[] }); })),
      visitas: this.visitasService.listar(1, 100).pipe(catchError(() => { this.erroCarregamento = true; return of({ total: 0, items: [] } as { total: number; items: { dataVisita: string }[] }); })),
      comunidades: this.comunidadesService.listar(1, 100).pipe(catchError(() => { this.erroCarregamento = true; return of({ total: 0, items: [] } as { total: number; items: { nome: string; totalBeneficiarios: number }[] }); })),
    }).subscribe(({ beneficiarios, doadores, visitas, comunidades }) => {
      this.cards[0].value = beneficiarios.total;
      this.cards[1].value = doadores.total;
      this.cards[3].value = comunidades.total;
      this.totalComunidades = comunidades.total;
      this.comunidades = comunidades.items;
      this.donutGradient = this.gerarGradienteDoNut(comunidades.items);

      const hoje = new Date();
      this.cards[2].value = visitas.items.filter(item => {
        const data = new Date(item.dataVisita);
        return data.getMonth() === hoje.getMonth() && data.getFullYear() === hoje.getFullYear();
      }).length;

      this.evolucaoMensal = this.gerarEvolucaoMensal(beneficiarios.items, doadores.items);
      this.carregando = false;
      this.changeDetectorRef.markForCheck();
    });
  }

  alturaBarra(valor: number): number {
    const maior = Math.max(...this.evolucaoMensal.map(item => Math.max(item.beneficiarios, item.doacoes)), 1);
    return Math.max(4, (valor / maior) * 100);
  }

  private gerarEvolucaoMensal(
    beneficiarios: { dataCriacao: string }[],
    doadores: { dataCadastro: string }[],
  ): MonthlyData[] {
    const hoje = new Date();
    const resultado: MonthlyData[] = [];
    const nomesMeses = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

    for (let offset = 5; offset >= 0; offset--) {
      const data = new Date(hoje.getFullYear(), hoje.getMonth() - offset, 1);
      const beneficiariosMes = beneficiarios.filter(item => {
        const cadastro = new Date(item.dataCriacao);
        return cadastro.getMonth() === data.getMonth() && cadastro.getFullYear() === data.getFullYear();
      }).length;

      const doadoresMes = doadores.filter(item => {
        const cadastro = new Date(`${item.dataCadastro}T00:00:00`);
        return cadastro.getMonth() === data.getMonth() && cadastro.getFullYear() === data.getFullYear();
      }).length;

      resultado.push({
        label: nomesMeses[data.getMonth()],
        beneficiarios: beneficiariosMes,
        doacoes: doadoresMes,
      });
    }

    return resultado;
  }

  private gerarGradienteDoNut(comunidades: { nome: string; totalBeneficiarios: number }[]): string {
    const total = comunidades.reduce((soma, item) => soma + (item.totalBeneficiarios || 0), 0);
    if (total <= 0) return 'conic-gradient(#14b8a6 0 100%)';

    const cores = ['#14b8a6', '#3b82f6', '#f59e0b', '#22c55e', '#a855f7', '#ec4899'];
    let inicio = 0;
    const segmentos = comunidades
      .filter(item => item.totalBeneficiarios > 0)
      .map((item, index) => {
        const fim = inicio + (item.totalBeneficiarios / total) * 100;
        const segmento = `${cores[index % cores.length]} ${inicio.toFixed(2)}% ${fim.toFixed(2)}%`;
        inicio = fim;
        return segmento;
      });

    return `conic-gradient(${segmentos.join(', ')})`;
  }
}
