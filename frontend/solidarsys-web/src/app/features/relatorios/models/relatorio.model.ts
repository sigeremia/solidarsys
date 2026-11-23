export type PeriodoRelatorio = 'mensal' | 'trimestral' | 'semestral' | 'anual';

export interface RelatorioPonto {
  /** yyyy-MM-dd — dia (período mensal) ou primeiro dia do mês */
  data: string;
  beneficiarios: number;
  doacoes: number;
}

export interface RelatorioComunidade {
  nome: string;
  totalBeneficiarios: number;
}

export interface Relatorio {
  periodo: PeriodoRelatorio;
  granularidade: 'DIA' | 'MES';
  inicio: string;
  fim: string;
  totalBeneficiarios: number;
  totalDoacoes: number;
  comunidadesAtendidas: number;
  semDados: boolean;
  serie: RelatorioPonto[];
  comunidades: RelatorioComunidade[];
}
