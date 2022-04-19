using System;
using System.Collections.Generic;
using Xunit;

namespace Tdd.Tests
{
    public class Dolar
    {
        public int total;
        public Dolar(int total)
        {
            this.total = total;
        }

        public void Multiplicar(int multiplicador)
        {
            total = total * multiplicador;
        }
    }


    public class Class1
    {
        [Fact]
        public void DeveCalcularAdicaoComMoedasDiferentes()
        {
            var cincoDolares = Dinheiro.Dolar(5);
            IOperacao dezReais = Dinheiro.Real(10);

            var banco = new Banco();
            banco.AddCambio("BRL", "USD", 2);
            var resultado = banco.Processar(cincoDolares.Soma(dezReais), "USD");
            Assert.Equal(Dinheiro.Dolar(10), resultado);
        }

        [Fact]
        public void DeveCalcularTaxaMesmaMoeda()
        {
            Assert.Equal(1, new Banco().CalcularTaxa("USD", "USD"));
        }

        [Fact]
        public void DeveProcessarSomaEmMoedasDiferentes()
        {
            var banco = new Banco();
            banco.AddCambio("BRL", "USD", 5);
            Dinheiro resultado = banco.Processar(Dinheiro.Real(5), "USD");
            Assert.Equal(Dinheiro.Dolar(1), resultado);
        }

        [Fact]
        public void DeveProcessarSoma()
        {
            IOperacao soma = new Soma(Dinheiro.Dolar(3), Dinheiro.Dolar(4));
            Banco banco = new Banco();
            Dinheiro resultado = banco.Processar(soma, "USD");
            Assert.Equal(Dinheiro.Dolar(7), resultado);
        }

        [Fact]
        public void DeveProcessarOperacaoDinheiro()
        {
            var banco = new Banco();
            var resultado = banco.Processar(Dinheiro.Dolar(1), "USD");
            Assert.Equal(Dinheiro.Dolar(1), resultado);
        }

        [Fact]
        public void TestarAdicaoRetornandoSoma()
        {
            Dinheiro cinco = Dinheiro.Dolar(5);
            IOperacao resultado = cinco.Soma(cinco);
            Soma soma = (Soma)resultado;
            Assert.Equal(cinco, soma.Augendo);
            Assert.Equal(cinco, soma.Adendo);

        }

        [Fact]
        public void TestarAdicao()
        {
            // $640.00 + R$ 25000,00 = $5640.00
            // com taxa de 5:1
            var valor = Dinheiro.Dolar(5);
            IOperacao soma = valor.Soma(valor);

            var banco = new Banco();
            Dinheiro resultado = banco.Processar(soma, "USD");
            Assert.Equal(Dinheiro.Dolar(10), resultado);
        }

        [Fact]
        public void DeveMultiplicarTotal()
        {
            // $320 * 2 = $640
            Dinheiro valorBase = Dinheiro.Dolar(320);
            Assert.Equal(Dinheiro.Dolar(640), valorBase.Multiplicar(2));

            //efeitos colaterais da modelagem do objeto dolar,
            //o valor é alterado
            Assert.Equal(Dinheiro.Dolar(960), valorBase.Multiplicar(3));
        }

        [Fact]
        public void DeveMultiplicarRealTotal()
        {
            // 5000 * 5 = R$ 25000
            Dinheiro valorBase = Dinheiro.Real(5000);
            Assert.Equal(Dinheiro.Real(25000), valorBase.Multiplicar(5));

            //efeitos colaterais da modelagem do objeto real,
            //o valor é alterado
            Assert.Equal(Dinheiro.Real(10000), valorBase.Multiplicar(2));
        }

        //precisamos conseguir comparar instâncias dos objetos
        [Fact]
        public void DeveSerIgual()
        {
            Assert.True(Dinheiro.Dolar(5).Equals(Dinheiro.Dolar(5)));
            Assert.True(Dinheiro.Real(5).Equals(Dinheiro.Real(5)));
        }

        [Fact]
        public void DeveSerDiferente()
        {
            Assert.False(Dinheiro.Dolar(5).Equals(Dinheiro.Dolar(6)));
            Assert.False(Dinheiro.Real(5).Equals(Dinheiro.Real(6)));
            Assert.False(Dinheiro.Dolar(5).Equals(Dinheiro.Real(5)));
        }

        [Fact]
        public void DeveCompararMoeda()
        {
            Assert.Equal("USD", Dinheiro.Dolar(1).ObterMoeda());
            Assert.Equal("BRL", Dinheiro.Real(1).ObterMoeda());
        }
    }

    public class Soma : IOperacao
    {
        public IOperacao Augendo;
        public IOperacao Adendo;

        public Soma(IOperacao augendo, IOperacao adendo)
        {
            Augendo = augendo;
            Adendo = adendo;
        }

        public Dinheiro Processar(Banco banco, string destino)
        {
            int total = Augendo.Processar(banco, destino).total + Adendo.Processar(banco, destino).total;
            return new Dinheiro(total, destino);
        }
    }

    public interface IOperacao
    {
        Dinheiro Processar(Banco banco, string destino);
    }

    public class Banco
    {
        private Dictionary<Pair, int> _taxas;

        public Banco()
        {
            _taxas = new Dictionary<Pair, int>();
        }
        public Dinheiro Processar(IOperacao origem, string destino)
        {
            return origem.Processar(this, destino);
        }

        public int CalcularTaxa(string origem, string destino)
        {
            if (origem.Equals(destino))
                return 1;

            return _taxas[new Pair(origem, destino)];
        }

        public void AddCambio(string origem, string destino, int taxa)
        {
            _taxas.Add(new Pair(origem, destino), taxa);
        }

        private class Pair
        {
            public string Origem { get; private set; }
            public string Destino { get; private set; }

            public Pair(string origem, string destino)
            {
                this.Origem = origem;
                this.Destino = destino;
            }

            public override bool Equals(object obj)
            {
                var pair = (Pair)obj;
                return Origem.Equals(pair.Origem) && Destino.Equals(pair.Destino);
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }
    }

    

    public class Dinheiro :  IOperacao
    {
        public int total { get; private set; }
        public string moeda;

        public IOperacao Multiplicar(int multiplicador)
        {
            return new Dinheiro(total * multiplicador, moeda);
        }

        public IOperacao Soma(IOperacao adendo)
        {
            return new Soma(this, adendo);
        }

        public Dinheiro Processar(Banco banco, string destino)
        {
            var taxa = banco.CalcularTaxa(this.moeda, destino);
            return new Dinheiro(total / taxa, destino);
        }

        public Dinheiro(int total, string moeda)
        {
            this.total = total;
            this.moeda = moeda;
        }

        public static Dinheiro Dolar(int total)
        {
            return new Dinheiro(total, "USD");
        }

        public static Dinheiro Real(int total)
        {
            return new Dinheiro(total, "BRL");
        }

        public string ObterMoeda()
        {
            return moeda;
        }

        public override bool Equals(object obj)
        {
            var dinheiro = (Dinheiro)obj;
            return total == dinheiro.total && this.moeda.Equals(dinheiro.moeda);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }


   
}
