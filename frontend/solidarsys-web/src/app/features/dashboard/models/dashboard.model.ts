export interface EvolucaoMensal {
  ano: number;
  mes: number;
  beneficiarios: number;
  doadores: number | null;
}

export interface ComunidadeResumo {
  nome: string;
  totalBeneficiarios: number;
}

export interface DashboardResumo {
  totalBeneficiarios: number;
  totalDoadores: number | null;
  visitasMes: number;
  totalComunidades: number;
  evolucaoMensal: EvolucaoMensal[];
  comunidades: ComunidadeResumo[];
}
