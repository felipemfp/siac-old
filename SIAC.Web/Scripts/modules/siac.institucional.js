﻿siac.Institucional = siac.Institucional || {};

siac.Institucional.Configuracao = (function () {
	function iniciar() {

		$('.ui.dropdown').dropdown();
		$('.ui.accordion').accordion({ animateChildren: false });

		$('.tabular.menu .item').tab({
			history: true,
			historyType: 'state',
			path: '/institucional/configuracao'
		});
	}

	return {
		iniciar: iniciar
	}
})();

siac.Institucional.Gerar = (function () {
    function iniciar() {
        $('.ui.modal').modal();
        $('.ui.termo.modal').modal({ closable: false }).modal('show');
        $('.cancelar.button').popup({ on: 'click' });
        $('.ui.confirmar.modal')
         .modal({
             onApprove: function () {
                 $('form').submit();
             }
         });

        $('.prosseguir.button').click(function () {
            prosseguir();
        });
    }

    function prosseguir() {
        var $errorList = $('form .error.message .list');

        $errorList.html('');
        $('form').removeClass('error');

        var valido = true;

        if (!$('#txtTitulo').val().trim()) {
            $errorList.append('<li>Insira o título</li>');
            valido = false;
        }

        if (!($('#txtObjetivo').val().trim())) {
            $errorList.append('<li>Insira o objetivo</li>');
            valido = false;
        }

        if (valido) {
            confirmar();
        }
        else {
            $('form').addClass('error');
            $('html, body').animate({
                scrollTop: $('form .error.message').offset().top
            }, 1000);
        }
    }

    function confirmar() {
        $modal = $('.ui.confirmar.modal');

        $('#txtModalTitulo').val($('#txtTitulo').val());
        $('#txtModalObjetivo').val($('#txtObjetivo').val());

        $modal.modal('show');
    }

    return {
        iniciar: iniciar
    }
})();

siac.Institucional.Configurar = (function () {
    var _codAvaliacao;

    function iniciar() {
        _codAvaliacao = window.location.pathname.toLowerCase().match(/avi[0-9]+$/)[0];
        $('.ui.dropdown').dropdown();
        $('.tabular.menu .item').tab();
        $('h3').popup();
        $('.ui.accordion').accordion({ animateChildren: false });
        $('.floated.remover.button').popup({ on: 'click', inline: true });
        $('.floated.editar.button').click(function () {
            abrirModalEditar(this);
        })
        $('.tab.questoes .remover.button.tiny').click(function () {
            deletarQuestao(this);
        });
        $('.cancelar.button').popup({ on: 'click', inline: true });
        $('.ui.checkbox').checkbox({
            onChange: function () {
                var checked = $(this).is(':checked');
                if (checked) {
                    $('#txtAlternativaDiscursiva').prop('readonly', false).parent().removeClass('disabled');

                } else {
                    $('#txtAlternativaDiscursiva').prop('readonly', true).parent().addClass('disabled');
                }
            }
        });
        $('#ddlModulo').dropdown('set selected', 1);
        $('#ddlCategoria').dropdown('set selected', 1);
        $('#ddlIndicador').dropdown('set selected', 1);
        $('#ddlTipo').dropdown('set selected', 2);
        $('#txtEnunciado').val('Ainda assim, existem dúvidas a respeito de como a mobilidade dos capitais internacionais cumpre um papel essencial na formulação dos modos de operação convencionais.');


        //Definindo o Tipo Default como 1 ( Objetiva )
        //$('#ddlTipo').dropdown('set selected', 1);

        //Adicionando Alternativas por default
        for (var i = 0; i < 5; i++) {
            adicionarAlternativa();
        }

        //Funções
        $('#ddlTipo').change(function () {
            mostrarCamposPorTipo();
        });

        $('.questao.objetiva .adicionar.button').click(function () {
            adicionarAlternativa();
        });

        $('.adicionar.questao.button').click(function () {
            verificar();
        })
    }

    function adicionarAlternativa() {
        var i = $('.ui.alternativas.accordion .title').length + 1;
        $('.ui.alternativas.accordion').append(
                        '<div class="title">' +
                            '<input id="txtAlternativaIndex" value="' + i + '" hidden />' +
                            '<i class="dropdown icon"></i>Alternativa ' + i +
                        '</div>' +
                        '<div class="content ui segment">' +
                            '<div class="field required">' +
                                '<label for="txtAlternativaEnunciado' + i + '">Enunciado</label>' +
                                '<textarea id="txtAlternativaEnunciado' + i + '" name="txtAlternativaEnunciado' + i + '" rows="2" placeholder="Enunciado..."></textarea>' +
                            '</div>' +
                            '<div class="field">' +
                                '<div class="ui button">' +
                                    'Remover' +
                                '</div>' +
                                '<div class="ui special popup">' +
                                    '<div class="header">Tem certeza?</div>' +
                                    '<div class="content"><p>Essa ação não poderá ser desfeita.</p>' +
                                    '<div class="ui right aligned remover button tiny">Sim, remover</div>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                    '</div>');
        $('#txtQtdAlternativas').val(i);
        $('.ui.alternativas.accordion .remover.button').off().click(function () {
            removerAlternativa(this);
        });
        $('.ui.alternativas.accordion .button').popup({ inline: true, on: 'click', position: 'right center' });
        $('.ui.alternativas.accordion').accordion({ animateChildren: false });

    }

    function renomearAlternativas() {
        var list = $('.ui.alternativas.accordion .title');
        var listContent = $('.ui.alternativas.accordion .content.segment');
        for (var i = 0; i < list.length; i++) {
            var j = list.eq(i).find('#txtAlternativaIndex').val();
            list.eq(i).html('<input id="txtAlternativaIndex" value="' + (i + 1) + '" hidden /><i class="dropdown icon"></i>Alternativa ' + (i + 1));

            /* RENOMEAR LABELS, INPUTS e TEXTAREAS */
            listContent.eq(i).find('label[for="txtAlternativaEnunciado' + j + '"]').attr('for', 'txtAlternativaEnunciado' + (i + 1));
            listContent.eq(i).find('textarea[name="txtAlternativaEnunciado' + j + '"]').attr('name', 'txtAlternativaEnunciado' + (i + 1)).attr('id', 'txtAlternativaEnunciado' + (i + 1));
        }
    }

    function removerAlternativa(button) {
        var i = $('.ui.alternativas.accordion .title').length;
        if (i > 2) {
            var content = $(button).parent().parent().parent().parent();
            var title = $(content).prev();
            title.remove();
            content.remove();
            renomearAlternativas();
            i--;
        } else {
            siac.aviso('É preciso ter, no mínimo, duas alternativas por questão', 'red');
        }
        $('#txtQtdAlternativas').val(i);
    }

    function mostrarCamposPorTipo() {
        var tipo = $('#ddlTipo').val();
        $('.questao.objetiva').hide();
        if (tipo == 1) {
            $('.questao.objetiva').show();
            $('.segment.questao.objetiva').attr('style', '');
        }
        else if (tipo == 2) {
            $('.questao.objetiva').hide();
        }
    }

    function verificar() {
        $('form.cadastro').removeClass('error');
        $('.ui.error.message .list').html('');
        var validado = false;
        if (!$('#ddlModulo').val()) {
            $('.ui.error.message .list').append('<li>É necessário selecionar um módulo</li>')
        }
        if (!$('#ddlCategoria').val()) {
            $('.ui.error.message .list').append('<li>É necessário selecionar uma categoria</li>')
        }
        if (!$('#ddlIndicador').val()) {
            $('.ui.error.message .list').append('<li>É necessário selecionar um indicador</li>')
        }

        if (!$('#ddlTipo').val()) {
            $('.ui.error.message .list').append('<li>É necessário selecionar um tipo</li>')
        }

        if (!$('#txtEnunciado').val()) {
            $('.ui.error.message .list').append('<li>É necessário preencher o enunciado</li>')
        }

        if ($('#ddlTipo').val()) {
            var tipo = $('#ddlTipo').val();

            if (tipo == 1) {
                var qteAlternativas = $('#txtQtdAlternativas').val();
                var ok = true;
                for (var i = 0; i < qteAlternativas; i++) {
                    if ($('#txtAlternativaEnunciado' + (i + 1)).val() == '') {
                        ok = false;
                    }
                }
                if ($('#chkAlternativaDiscursiva').is(':checked')) {
                    if (!$('#txtAlternativaDiscursiva').val()) {
                        ok = false;
                    }
                }
                if (ok) {
                    validado = true;
                }
                else {
                    $('.ui.error.message .list').append('<li>É necessário preencher os enunciados de todas as alternativas</li>')
                }
            }
            else if (tipo == 2) {
                if ($('#txtEnunciado').val()) {
                    validado = true;
                }
            }
        }

        if (validado) {
            inserirQuestao();
        }
        else $('form.cadastro').addClass('error');
    }

    function adicionarQuestao(questaoId) {
        //OBTENDO DADOS DA QUESTÃO
        var moduloCod = $('#ddlModulo :selected').val();
        var modulo = $('#ddlModulo :selected').text();
        var categoriaCod = $('#ddlCategoria').val();
        var categoria = $('#ddlCategoria :selected').text();
        var indicadorCod = $('#ddlIndicador').val();
        var indicador = $('#ddlIndicador :selected').text();
        var tipo = $('#ddlTipo').val();
        var enunciado = $('#txtEnunciado').val();
        var observacao = $('#txtObservacao').val();

        var TEMPLATE_QUESTAO_HTML = '<div class="title">' +
                                        '<i class="dropdown icon"></i>' +
                                        modulo +
                                    '</div>' +
                                    '<div class="content" data-modulo="' + moduloCod + '">' +
                                        '<div class="accordion">' +
                                            '<div class="title">' +
                                                '<i class="dropdown icon"></i>' +
                                                categoria +
                                            '</div>' +
                                            '<div class="content" data-categoria="' + categoriaCod + '">' +
                                                '<div class="accordion">' +
                                                    '<div class="title">' +
                                                        '<i class="dropdown icon"></i>' +
                                                         indicador +
                                                    '</div>' +
                                                    '<div class="content" data-indicador="' + indicadorCod + '">' +
                                                        '<div class="accordion">' +
                                                            '<div class="title">' +
                                                                '<i class="dropdown icon"></i>' +
                                                                     enunciado.encurtarTextoEm(50) +
                                                            '</div>' +
                                                            '<div class="content" data-questao="' + questaoId + '">' +
                                                                '<div class="ui segment">' +
                                                                    '<div class="ui right floated remover button">Remover</div>' +
                                                                    '<div class="ui popup">' +
                                                                        '<div class="header">Tem certeza?</div>' +
                                                                        '<div style="padding:0!important;" class="content">' +
                                                                            '<p>Essa ação não poderá ser desfeita.</p>' +
                                                                            '<div class="ui right aligned remover button tiny">Sim, remover</div>' +
                                                                        '</div>' +
                                                                    '</div>' +
                                                                    '<div class="ui right floated editar button">Editar</div>' +
                                                                    '<h3 class="ui dividing header" data-content="' + observacao + '">' + enunciado + '</h3>' +
                                                                    '<div class="ui very relaxed list">' +
                                                                    //    <div class="item">\
                                                                    //        <b>1)</b> ALternativa\
                                                                    //    </div>\
                                                                    '</div>' +
                                                                '</div>' +
                                                            '</div>' +
                                                        '</div>' +
                                                    '</div>' +
                                                '</div>' +
                                            '</div>' +
                                        '</div>' +
                                    '</div>';
        var $template = $(TEMPLATE_QUESTAO_HTML);

        if (tipo == 1) {
            var list = $('.ui.alternativas.accordion .title');
            var listContent = $('.ui.alternativas.accordion .content.segment');
            for (var i = 0; i < list.length; i++) {
                var j = list.eq(i).find('#txtAlternativaIndex').val();

                var enunciado = listContent.eq(i).find('textarea[name="txtAlternativaEnunciado' + j + '"]').val();
                var alternativa = '<div class="item"><b>' + j + ')</b> ' + enunciado + '</div>';
                $template.find('.very.relaxed.list').append(alternativa);
            }
            if ($('#chkAlternativaDiscursiva').is(':checked')) {
                var alternativaDiscursiva = $('#txtAlternativaDiscursiva').val();
                var alternativa = '<div class="item"><b>' + (list.length + 1) + ')</b> ' + alternativaDiscursiva + '<div class="ui left pointing label">Alternativa discursiva</div></div>';
                $template.find('.very.relaxed.list').append(alternativa);
            }
        }
        else if (tipo == 2) {
            $template.find('.very.relaxed.list').remove();
            $template.find('.ui.segment').append('<input class="input" placeholder="Resposta" type="text" readonly/>');
        }

        //Obtendo possíveis accordions questões por módulos
        var $questoesModulo = $('[data-modulo=' + moduloCod + ']');
        var $questoesCategoria = $('[data-categoria=' + categoriaCod + ']');
        var $questoesIndicador = $('[data-indicador=' + indicadorCod + ']');

        var $local = $('.tab.questoes .fluid.styled.accordion');
        var $questao = $template;
        var $localNovo = $questoesModulo;
        var questaoIndice = 1;

        if ($localNovo.length > 0) {
            $local = $localNovo;
            $localNovo = $local.find('[data-categoria=' + categoriaCod + ']');
            questaoIndice = $local.length + 1;
            if ($localNovo.length > 0) {
                $local = $localNovo;
                $localNovo = $local.find('[data-indicador=' + indicadorCod + ']');
                questaoIndice = $local.length + 1;
                if ($localNovo.length > 0) {
                    $local = $localNovo;
                    $questao = $template.find('[data-indicador=' + indicadorCod + ']').find('.accordion').html();
                } else {
                    $questao = $template.find('[data-categoria=' + categoriaCod + ']').find('.accordion').html();
                }
            }
            else {
                $questao = $template.find('.accordion').html();
            }
            $local.children('.accordion').append($questao);
        }
        else {
            $local.append($questao);
        }
        $('h3').popup();
        $('.floated.remover.button').popup({ on: 'click', inline: true });
        $('.tab.questoes .remover.button.tiny').click(function () {
            deletarQuestao(this);
        });
        $('.tab.questoes .editar.button').click(function () {
            abrirModalEditar(this);
        });
        $('.ui.accordion').accordion({ animateChildren: false });
        siac.aviso('Questão adicionada com sucesso!', 'green');
    }

    function removerQuestao(button) {
        var content = $(button).parent().parent().parent().parent();
        var title = $(content).prev();
        var modulo = content.parents('[data-modulo]');
        var categoria = content.parents('[data-categoria]');
        var indicador = content.parents('[data-indicador]');
        //var accordionQuestoes = content.parent();
        title.remove();
        content.remove();

        if (indicador.find('.title').length <= 0) {
            indicador.prev().remove();
            indicador.remove();
            if (categoria.find('.title').length <= 0) {
                categoria.prev().remove();
                categoria.remove();
                if (modulo.find('.title').length <= 0) {
                    modulo.prev().remove();
                    modulo.remove();
                }
            }
        }
        siac.aviso('Questão removida com sucesso!', 'red');
    }

    function inserirQuestao() {
        $('.adicionar.questao.button').addClass('loading');
        var form = $('form.cadastro').serialize();
        $.ajax({
            type: 'POST',
            url: '/institucional/CadastrarQuestao/' + _codAvaliacao,
            data: form,
            dataType: 'json',
            success: function (questaoId) {
                adicionarQuestao(questaoId);
            },
            error: function () {
                siac.mensagem('Erro na adição de questão');
            },
            complete: function () {
                $('.adicionar.questao.button').removeClass('loading');
            }
        });
    }

    function deletarQuestao(button) {
        var content = $(button).parent().parent().parent().parent();

        var modulo = content.parents('[data-modulo]').data('modulo');
        var categoria = content.parents('[data-categoria]').data('categoria');
        var indicador = content.parents('[data-indicador]').data('indicador');
        var ordem = content.data('questao');

        $(button).addClass('loading');
        $.ajax({
            type: 'POST',
            url: '/institucional/RemoverQuestao/' + _codAvaliacao,
            data: {
                modulo: modulo,
                categoria: categoria,
                indicador: indicador,
                ordem: ordem
            },
            success: function () {
                removerQuestao(button);
            },
            error: function () {
                siac.mensagem('Erro na remoção de questão');
            },
            complete: function () {
                $(button).removeClass('loading');
            }
        });
    }

    function abrirModalEditar(button) {
        $('.ui.editar.modal .alternativas').html('');
        $('.ui.editar.modal form').removeClass('error');
        var $content = $(button).parent().parent();

        var enunciado = $content.find('h3').text();
        var observacao = $content.find('h3').data('content');

        $('#txtEditarEnunciado').val(enunciado);
        $('#txtEditarObservacao').val(observacao);

        var qteAlternativas = $content.find('.item').length;
        if (qteAlternativas > -1) {
            var indice = 1;
            
            $content.find('.item').map(function () {
                //Obter somente o texto do elemento 'DIV' Pai
                var alternativa = $(this).clone().children().remove().end().text().trim();
                
                var $ALTERNATIVA_TEMPLATE = $('<div class="field required">' +
                                                '<label for="txtEditarAlternativa' + indice + '">Alternativa '+indice+'</label>' +
                                                '<textarea id="txtEditarAlternativa' + indice + '" name="txtEditarAlternativa' + indice + '" rows="2" required placeholder="Alternativa...">'+alternativa+'</textarea>' +
                                            '</div>');

                //Caso a alternativa seja discursiva
                if ($(this).find('.label').length > 0) {
                    $ALTERNATIVA_TEMPLATE.find('label').text('Alternativa Discursiva').attr('for', 'txtEditarAlternativaDiscursiva');
                    $ALTERNATIVA_TEMPLATE.find('textarea').attr('id', 'txtEditarAlternativaDiscursiva').attr('name', 'txtEditarAlternativaDiscursiva');
                }

                $('.ui.editar.modal .alternativas').append($ALTERNATIVA_TEMPLATE);
                indice++;
            })
        }

        var modulo = $content.parents('[data-modulo]').data('modulo');
        var categoria = $content.parents('[data-categoria]').data('categoria');
        var indicador = $content.parents('[data-indicador]').data('indicador');
        var ordem = $content.data('questao');
        $('.ui.editar.modal').modal({
            onApprove: function () {
                $modal = $(this);
                var validado = true;
                $modal.find(':input').map(function () {
                    if (!$(this).val() && $(this).attr('id') != 'txtEditarObservacao') {
                        $modal.find('.error.message .list').html('<li>Todos os campos obrigatórios devem ser preenchidos!');
                        $modal.find('form').addClass('error');
                        validado = false;
                    }
                });
                if (validado) {
                    $modal.find('form').removeClass('error');
                    editarQuestao(modulo, categoria, indicador, ordem, $content);
                }
                return false;
            }
        }).modal('show');   
    }

    function editarQuestao(modulo, categoria, indicador, ordem,$content) {
        var $button = $('.editar.modal .approve.button');
        $button.addClass('loading');
        var form = $('form.edicao').serialize();
        //$.extend(form, { modulo: modulo });
        $.ajax({
            type: 'POST',
            url: '/institucional/EditarQuestao/' + _codAvaliacao+'?modulo='+modulo+'&categoria='+categoria+'&indicador='+indicador+'&ordem='+ordem,
            data: form,
            dataType: 'json',
            success: function (form) {
                $content.find('h3').attr('data-content', $('#txtEditarObservacao').val()).popup().text($('#txtEditarEnunciado').val());

                qteAlternativa = $content.find('.relaxed.list .item').length;

                if (qteAlternativa > 0) {
                    var indice = 1;
                    $content.find('.relaxed.list .item').map(function () {
                        var $alternativa = $(this);
                        var alternativaFormTexto = '';

                        if ($alternativa.find('.label').length > 0) { //Caso seja discursiva
                            alternativaFormTexto = $('#txtEditarAlternativaDiscursiva').val();
                            $alternativa.html('<b>' + indice + ')</b> ' + alternativaFormTexto + '<div class="ui left pointing label">Alternativa discursiva</div>');
                        } else { //Caso seja objetiva
                            alternativaFormTexto = $('#txtEditarAlternativa' + indice).val();
                            $alternativa.html('<b>' + indice + ')</b> ' + alternativaFormTexto);
                        }
                        indice++;
                    });
                }

            },
            error: function (erro) {
                siac.mensagem('Erro na edição de questão');
                alert(erro);
            },
            complete: function () {
                $button.removeClass('loading');
                $('.ui.editar.modal').modal('hide');
            }
        });
    }

    return {
        iniciar: iniciar
    }
})();