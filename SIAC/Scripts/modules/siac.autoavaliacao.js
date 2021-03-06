﻿/*
This file is part of SIAC.

Copyright (C) 2016 Felipe Mateus Freire Pontes <felipemfpontes@gmail.com>
Copyright (C) 2016 Francisco Bento da Silva Júnior <francisco.bento.jr@hotmail.com>

SIAC is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details. 
*/
siac.Autoavaliacao = siac.Autoavaliacao || {};

siac.Autoavaliacao.Index = (function () {
    var _controleTimeout, _controlePartial, _controleQte = 12, _controleAjax;

    var pagina = 1;
    var ordenar = "data_desc";
    var categoria = "";
    var dificuldade = "";
    var disciplina = "";
    var pesquisa = "";

    function iniciar() {
        $(window).scroll(function () {
            if ($(window).scrollTop() + $(window).height() > $(document).height() * 0.5) {
                if ($('.cards .card').length == (_controleQte * pagina)) {
                    pagina++;
                    listar();
                }
            }
        });

        $('.ui.dropdown').dropdown();

        $('.pesquisa input').keyup(function () {
            var _this = this;
            if (_controleTimeout) {
                clearTimeout(_controleTimeout);
            }
            _controleTimeout = setTimeout(function () {
                pesquisa = _this.value;
                pagina = 1;
                $(_this).closest('.input').addClass('loading');
                listar();
            }, 500);
        });

        $('.button.topo').click(function () {
            topo();
            return;
        });

        $('.categoria.item').click(function () {
            var $_this = $(this);
            pagina = 1;
            categoria = $_this.attr('data-categoria');
            $_this.closest('.segment').addClass('loading');
            listar();
        });

        $('.dificuldade.item').click(function () {
            var $_this = $(this);
            pagina = 1;
            dificuldade = $_this.attr('data-dificuldade');
            $_this.closest('.segment').addClass('loading');
            listar();
        });

        $('.disciplina.item').click(function () {
            var $_this = $(this);
            pagina = 1;
            disciplina = $_this.attr('data-disciplina');
            $_this.closest('.segment').addClass('loading');
            listar();
        });

        $('.ordenar.item').click(function () {
            var $_this = $(this);
            pagina = 1;
            ordenar = $_this.attr('data-ordenar');
            $_this.closest('.segment').addClass('loading');
            listar();
            $('.ordenar.item').removeClass('active');
            $_this.addClass('active');
        });

        listar();
    }

    function listar() {
        if (_controleAjax && _controleAjax.readyState != 4) {
            _controleAjax.abort();
        }
        $cards = $('.ui.cards');
        $cards.parent().append('<div class="ui active centered inline text loader">Carregando</div>');
        _controleAjax = $.ajax({
            url: '/historico/autoavaliacao/listar',
            data: {
                pagina: pagina,
                ordenar: ordenar,
                dificuldade: dificuldade,
                disciplina: disciplina,
                categoria: categoria,
                pesquisa: pesquisa
            },
            method: 'POST',
            success: function (partial) {
                if (partial != _controlePartial) {
                    if (pagina == 1) {
                        $cards.html(partial);
                    }
                    else {
                        $cards.append(partial);
                    }
                    _controlePartial = partial;
                    $('.cards .card.hidden').transition({
                        animation: 'pulse',
                        duration: 500,
                        interval: 200
                    });
                }
            },
            complete: function () {
                $('.segment.loading').removeClass('loading');
                $('.input.loading').removeClass('loading');
                $cards.parent().find('.loader').remove();
                adicionarEventoArquivar();
            }
        });
    }

    function topo() {
        $("html, body").animate({
            scrollTop: 0
        }, 500);
        return false;
    }

    function adicionarEventoArquivar() {
        $('.arquivar.button').off("click").click(function () {
            var $this = $(this);
            var codAvaliacao = $this.parents('[data-avaliacao]').attr('data-avaliacao');
            $this.addClass('loading');
            $.ajax({
                type: 'POST',
                data: { codigo: codAvaliacao },
                url: '/principal/autoavaliacao/arquivar',
                success: function () {
                    window.location.href = '/historico/autoavaliacao/detalhe/' + codAvaliacao;
                },
                error: function () {
                    siac.mensagem('Não foi possível arquivar a autoavaliação.')
                    $this.removeClass('loading');
                }
            });
        });
    }

    return {
        iniciar: iniciar
    }
})();

siac.Autoavaliacao.Detalhe = (function () {
    var _codAvaliacao;

    function iniciar() {
        _codAvaliacao = window.location.pathname.toLowerCase().match(/auto[0-9]+$/)[0];

        $('.ui.accordion').accordion({
            animateChildren: false
        });

        $('.label, div').popup();

        $('.arquivar.button').click(function () {
            var $_this = $(this);
            $_this.addClass('loading');
            $.ajax({
                type: 'POST',
                data: { codigo: _codAvaliacao },
                url: '/principal/autoavaliacao/arquivar',
                success: function () {
                    window.location.href = '/historico/autoavaliacao/detalhe/' + _codAvaliacao;
                },
                error: function () {
                    siac.mensagem('Não foi possível arquivar a autoavaliação.')
                },
                complete: function () {
                    $(_this).removeClass('loading');
                }
            });
        });

        siac.Anexo.iniciar();
    }

    return {
        iniciar: iniciar
    }
})();

siac.Autoavaliacao.Resultado = (function () {
    function iniciar() {
        $('.ui.accordion')
            .accordion({
                animateChildren: false
            })
        ;
        $('.label, div')
            .popup()
        ;

        siac.Anexo.iniciar();
    }

    return {
        iniciar: iniciar
    }
})();

siac.Autoavaliacao.Realizar = (function () {
    var _codAvaliacao,
        _controleInterval,
        _controleRestante,
        _controleNoficacao;

    function iniciar() {
        _codAvaliacao = window.location.pathname.toLowerCase().match(/auto[0-9]+$/)[0];

        (function () {
            setInterval(function () {
                $.ajax({
                    type: 'GET',
                    url: '/acesso/conectado'
                });
            }, 1000 * 60 * 15);
        })();

        $('.ui.dropdown').dropdown();

        $('.visibility').visibility({
            continuous: true,
            onTopPassed: function () {
                $('.ui.dropdown').dropdown('set selected', this.innerHTML);
            },
            onBottomPassed: function () {
                $('.ui.dropdown').dropdown('set selected', this.innerHTML);
            }
        });

        $('.ui.sticky').sticky();

        $('a[href]').on('click', function () {
            $('.ui.confirmar.modal').modal('show');
            $('.ui.confirmar.modal #txtRef').val($(this).attr('href'));
            return false;
        });

        $('#chkCronometrar').change(function () {
            if ($(this).is(':checked')) {
                $('#txtDuracao').removeAttr('disabled').parent().removeClass('disabled')
            }
            else {
                $('#txtDuracao').attr('disabled', 'disabled').parent().addClass('disabled')
            }
        });

        $('.informacoes.button').click(function () {
            $('.ui.informacoes.modal').modal('show')
        });

        $('.ui.checkbox').checkbox();

        $('[data-alvo]').click(function () {
            $('html, body').animate({
                scrollTop: $('#' + $(this).data('alvo')).offset().top - 68
            }, 500);
        });

        //$('.ui.verificar.modal')
        //    .modal({
        //        closable: false,
        //        transition: 'vertical flip',
        //        onHide: function () {
        //            window.onbeforeunload = function () {
        //                return 'Você está realizando uma autoavaliação.';
        //            };
        //        }
        //    })
        //    .modal('show')
        //;

        $('.ui.cronometrar.modal')
            .modal({
                closable: false,
                onApprove: function () {
                    date = new Date();
                    $('#lblHoraInicio').text(("0" + (date.getHours())).slice(-2) + 'h' + ("0" + (date.getMinutes())).slice(-2) + 'min');
                    $('#lblHoraAgora').text(("0" + (date.getHours())).slice(-2) + 'h' + ('0' + (date.getMinutes())).slice(-2) + 'min');
                    setTimeout(relogio(), ((60 - date.getSeconds()) * 1000));
                    if ($('#chkCronometrar').is(':checked')) {
                        duracao = $('#txtDuracao').val();
                        $('#lblHoraDuracao').text(duracao.minutosParaStringTempo());
                        $('#lblHoraRestante').text(duracao.minutosParaStringTempo());
                        temporizador(new Date(date.getTime() + duracao * 60 * 1000));
                    }
                    else {
                        $('#lblHoraRestante').parent().remove();
                        $('#lblHoraDuracao').parent().remove();
                    }
                },
                onHide: function () {
                    window.onbeforeunload = function () {
                        return 'Você está realizando uma autoavaliação.';
                    };
                }
            })
            .modal('show')
        ;

        $('.ui.informacoes.modal')
            .modal()
        ;

        $('.ui.confirmar.modal')
            .modal({
                onApprove: function () {
                    window.onbeforeunload = function () {
                        $('.ui.global.loader').parent().dimmer('show');
                    };
                    window.location.href = $('.ui.confirmar.modal #txtRef').val();
                },
                onDeny: function () {
                    $('.ui.segment.loading').removeClass('loading');
                }
            })
        ;

        $('.ui.accordion')
            .accordion({
                animateChildren: false
            })
        ;

        $('.arquivar.button').click(function () {
            arquivar();
        });

        $('.finalizar.button').click(function () {
            finalizar();
        });

        siac.Anexo.iniciar();

        $('textarea[name^="txtResposta"], input[name^="rdoResposta"]').change(function () {
            var $_this = $(this),
                $checked = $('input[name=' + $_this.attr('name') + ']:checked');

            $label = $_this.closest('.segment').find('.ui.ribbon.label');

            if (($_this.attr('name').indexOf('rdo') > -1 && $checked.length) || ($_this.attr('name').indexOf('txt') > -1 && $_this.val())) {
                $label.removeClass('red');
                $label.removeAttr('style');
                $label.addClass('green');
                $label.html('Respondida');
                if ($_this.attr('name').indexOf('rdo') > -1) {
                    $label.find('.detail').remove();
                    $label.append($('<div class="detail"></div>').text($checked.next().find('b').text()));
                }
            }
            else {
                $label.attr('style', 'display:none');
            }
        }).change();

        $('textarea[name^="txtResposta"], input[name^="rdoResposta"], textarea[name^="txtComentario"]').change(function () {
            var $questao = $(this).parents('.questao.segment');
            var $resposta = $questao.find('textarea[name^="txtResposta"], input[name^="rdoResposta"]').eq(0);
            var $comentario = $questao.find('textarea[name^="txtComentario"]');
            if ($resposta.val().trim()) {
                questao = $questao.attr('id');
                resposta = $resposta.val().trim();
                comentario = $comentario.val().trim();
                salvarResposta(questao, resposta, comentario);
            }
        });

        $('.ui.gabarito.modal')
            .modal({
                onApprove: function () {
                    $.ajax({
                        type: 'GET',
                        url: "/acesso/conectado",
                        beforeSend: function () {
                            $('.ui.gabarito.modal .approve.button').addClass('loading');
                        },
                        success: function () {
                            window.onbeforeunload = function () {
                                $('.ui.global.loader').parent().dimmer('show');
                            };
                            $('form').submit();
                            $('.ui.gabarito.modal').modal('hide');
                        },
                        error: function () {
                            siac.mensagem('Conecte-se à internet antes de confirmar.')
                        },
                        complete: function () {
                            $('.ui.gabarito.modal .approve.button').removeClass('loading');
                        }
                    });
                    return false;
                }
            })
        ;
    }

    function salvarResposta(questao, resposta, comentario) {
        $.ajax({
            type: 'POST',
            url: '/principal/autoavaliacao/salvarresposta/' + _codAvaliacao,
            data: {
                questao: questao,
                resposta: resposta,
                comentario: comentario
            },
            beforeSend: function () {
                var not = siac.Lembrete.Notificacoes.exibir('Salvando respostas...');
                if (_controleNoficacao)
                    _controleNoficacao.dismiss();
                _controleNoficacao = not;
            },
            success: function () {
                var not = siac.Lembrete.Notificacoes.exibir('Respostas salvas.', 'positivo');
                if (_controleNoficacao)
                    _controleNoficacao.dismiss();
                _controleNoficacao = not;
            }
        })
    }

    function relogio() {
        setInterval(function () {
            var date = new Date();
            $('#lblHoraAgora').text(("0" + (date.getHours())).slice(-2) + 'h' + ("0" + (date.getMinutes())).slice(-2) + 'min');
        }, 1000);
    }

    function temporizador(dtTermino) {
        _controleInterval = setInterval(function () {
            var offset = dtTermino.getTimezoneOffset() * 60 * 1000;
            var timeRestante = (dtTermino.getTime() + offset) - (new Date().getTime() + offset);

            if (timeRestante > 0) {
                var date = new Date();
                date.setTime(timeRestante);
                var offsetDate = date.getTimezoneOffset() * 60 * 1000;
                date.setTime(date.getTime() + offsetDate);
                var txtRestante = ("0" + (date.getHours())).slice(-2) + 'h' + ("0" + (date.getMinutes())).slice(-2) + 'min';
                $('#lblHoraRestante').text(txtRestante);
                if (txtRestante != _controleRestante) {
                    $('#lblHoraRestante').parent().transition('flash');
                }
                _controleRestante = txtRestante;
                if (timeRestante < 1000 * 60 * 5 && !$('#lblHoraRestante').parent().hasClass('red')) {
                    $('#lblHoraRestante').parent().addClass('red');
                }
            }
            else {
                siac.mensagem('O tempo definido por você acabou.', 'O tempo acabou.');
                clearInterval(_controleInterval);
            }
        }, 1000);
    }

    function finalizar() {
        var finalizada = true;
        $objects = $('textarea[name^="txtResposta"], input[name^="rdoResposta"]');
        for (var i = 0, length = $objects.length; i < length; i++) {
            var _this = $objects.eq(i);
            $label = _this.closest('.segment').find('.ui.ribbon.label');
            if (_this.attr('name').indexOf('rdo') > -1) {
                if ($('input[name="' + _this.attr('name') + '"]:checked').length === 0) {
                    $label.removeAttr('style').addClass('red').html('Não respondida').transition('flash');
                    finalizada = false;
                }
            }
            else if (!_this.val()) {
                $label.removeAttr('style').addClass('red').html('Não respondida').transition('flash');
                finalizada = false;
            }
        }

        if (finalizada) {
            confirmar();
        }
        else {
            $('html, body').animate({
                scrollTop: $(".label.red.ribbon").closest('.segment').offset().top
            }, 500);
        }
    }

    function confirmar() {
        var $modal = $('.ui.gabarito.modal');
        var $basicSegment = $('form .ui.basic.segment').clone();

        $basicSegment.removeAttr('style');
        $modal.find('.content').html($('<div class="ui form"></div>').append($basicSegment));
        $modalBasicSegment = $modal.find('.ui.basic.segment');
        $modal.find('.ui.accordion').accordion({
            onChange: function () {
                $('.ui.gabarito.modal').modal('refresh');
            },
            animateChildren: false
        });

        var $lstOriginalTextarea = $('form .ui.basic.segment').find('textarea');
        var $lstCloneTextarea = $modalBasicSegment.find('textarea');
        for (var i = 0; i < $lstOriginalTextarea.length; i++) {
            $lstCloneTextarea.eq(i).val($lstOriginalTextarea.eq(i).val());
        }
        var $lstInput = $modalBasicSegment.find(':input,a');
        for (var i = 0; i < $lstInput.length; i++) {
            $lstInput.eq(i)
                .attr({
                    'readonly': 'readonly'
                })
                .removeAttr('id name href onchange onclick')
                .off()
            ;
        }
        $modal.modal('show');
    }

    function arquivar() {
        $.ajax({
            type: 'POST',
            data: { codigo: _codAvaliacao },
            url: "/principal/autoavaliacao/arquivar",
            success: function () {
                window.onbeforeunload = function () { $('.ui.global.loader').parent().dimmer('show'); };
                window.location.href = '/historico/autoavaliacao/detalhe/' + _codAvaliacao;
            },
            error: function () {
                siac.mensagem('Não foi possível arquivar a autoavaliação.')
            }
        });
    }

    return {
        iniciar: iniciar
    }
})();

siac.Autoavaliacao.Gerar = (function () {
    var _jsonDificuldades;

    function iniciar() {
        $json = $('code.json');
        _jsonDificuldades = JSON.parse($json.html());
        $json.remove();

        $('.ui.checkbox').checkbox();

        $('.ui.dropdown').dropdown();

        $('.ui.accordion').accordion({
            animateChildren: false
        });

        $('.prosseguir.button').click(function () {
            prosseguir();
        });

        $('.ui.confirmar.modal')
          .modal({
              onApprove: function () {
                  $('form').submit();
              }
          })
        ;

        $('#ddlTipo').change(function () {
            mostrarOpcoesPorTipo();
        });

        $('#ddlDisciplinas').parent().mouseout(function () {
            $('.ui.dropdown').dropdown();
        });

        $('#ddlDisciplinas').change(function () {
            ajustarFormulario();
        });

        $('.cancelar.button').popup({
            on: 'click'
        });
    }

    function prosseguir() {
        var validado = true;
        var usarPadrao = false;
        $list = $('form .error.message .list');
        $list.html('');

        if ($('#ddlDisciplinas :selected').length > 0 && $('#ddlDisciplinas :selected').val()) {
            var discs = $('#ddlDisciplinas').val();
            for (var i = 0; i < discs.length; i++) {
                if (!$('#ddlTemas' + discs[i] + ' :selected').length > 0) {
                    $list.append('<li>Selecione pelo menos um tema para ' + $('#ddlDisciplinas option[value="' + discs[i] + '"]').text() + '</li>');
                    validado = false;
                }

                if (!$('#ddlDificuldade' + discs[i]).val()) {
                    $list.append('<li>Selecione a dificuldade para ' + $('#ddlDisciplinas option[value="' + discs[i] + '"]').text() + '</li>');
                    validado = false;
                }
                else {
                    var qteObjetiva = $('#txtQteObjetiva' + discs[i]).val();
                    var qteDiscursiva = $('#txtQteDiscursiva' + discs[i]).val();

                    if (!$('#ddlTipo').val()) {
                        $list.html('<li>Selecione o tipo das questões.</li>');
                        validado = false;
                        usarPadrao = true;
                    }

                    else if ($('#ddlTipo').val() == '1') {
                        if (!qteObjetiva || qteObjetiva <= 0) {
                            validado = false;
                            usarPadrao = true;
                            $list.append('<li>Preencha a quantidade das questões para ' + $('#ddlDisciplinas option[value="' + discs[i] + '"]').text());
                        }
                    }
                    else if ($('#ddlTipo').val() == '2') {
                        if (!qteDiscursiva || qteDiscursiva <= 0) {
                            validado = false;
                            usarPadrao = true;
                            $list.append('<li>Preencha a quantidade das questões para ' + $('#ddlDisciplinas option[value="' + discs[i] + '"]').text());
                        }
                    }
                    else if ($('#ddlTipo').val() == '3') {
                        if (!qteObjetiva || !qteDiscursiva || qteObjetiva < 0 || qteDiscursiva < 0 || (qteObjetiva <= 0 && qteDiscursiva <= 0)) {
                            validado = false;
                            usarPadrao = true;
                            $list.append('<li>Preencha a quantidade das questões para ' + $('#ddlDisciplinas option[value="' + discs[i] + '"]').text());
                        }
                    }
                }
            }
        }
        else {
            $list.append('<li>Selecione pelo menos uma disciplina</li>');
            validado = false;
        }

        if (usarPadrao) {
            $list.append('<a id="btnUsarPadrao" href="">Prosseguir operação com valores padrão.</a>')

            var $btnUsarPadrao = $('#btnUsarPadrao');
            var $btnUsarPadraoPopUp = $('<div class="ui special wide popup">' +
                                        '<b>Valores Padrão</b>' +
                                        '<ul class="list">' +
                                            '<li>Quantidade das Questões: <b>10</b></li>' +
                                        '</ul>' +
                                      '</div>');

            if (!$('#ddlTipo').val()) {
                $btnUsarPadraoPopUp.find('ul').prepend('<li>Tipo das Questões: <b>Somente objetivas</b></li>');
            }

            $btnUsarPadrao.after($btnUsarPadraoPopUp);

            $btnUsarPadrao.popup({
                inline: true,
                position: 'right center'
            });

            $btnUsarPadrao.off('click').click(function () {
                usarValoresPadrao()
                return false;
            });
        }

        if (validado) {
            $('form.error').removeClass('error');
            $('#btnUsarPadrao').remove();
            confirmar()
        }
        else {
            $('form').addClass('error');
            $('html, body').animate({
                scrollTop: $('form .error.message').offset().top
            }, 1000);
        }
    }

    function recuperarTemasPorDisciplina(selecionado) {
        if (selecionado) {
            var ddlTema = $('#ddlTemas' + selecionado);
            $.ajax({
                cache: false,
                type: 'POST',
                url: '/tema/recuperartemasporcoddisciplinatemquestao',
                data: { "codDisciplina": selecionado },
                success: function (data) {
                    ddlTema.html('');
                    ddlTema.parent().find('.label').remove();
                    for (var i = 0, length = data.length; i < length; i++) {
                        ddlTema.append($('<option></option>').val(data[i].CodTema).html(data[i].Descricao));
                    }
                },
                error: function () {
                    siac.mensagem('Falha ao recuperar temas.');
                }
            });
        }
    }

    function ajustarFormulario() {
        var discs = $('#ddlDisciplinas').val();
        $('.ui.accordion').html('');
        var temAccordion = $('.ui.tema.accordion');
        var difAccordion = $('.ui.dificuldade.accordion');
        var qteAccordion = $('.ui.quantidade.accordion');
        var dificuldades;
        for (var j = 0, length = _jsonDificuldades.length; j < length; j++) {
            dificuldades += '<option value="' + _jsonDificuldades[j].Codigo + '">' + _jsonDificuldades[j].Descricao + '</option>';
        }
        for (var i = 0; i < discs.length; i++) {
            var disciplina = $('#ddlDisciplinas option[value="' + discs[i] + '"]').text();
            temAccordion.append('<div class="title"><i class="icon dropdown"></i>' + disciplina + '</div>' +
                                '<div class="content">' +
                                    '<div class="field">' +
                                        '<select required name="ddlTemas' + discs[i] + '" id="ddlTemas' + discs[i] + '" class="ui search dropdown" multiple>' +
                                            '<option value="">Tema</option>' +
                                        '</select>' +
                                    '</div>' +
                                '</div>');
            recuperarTemasPorDisciplina(discs[i]);

            difAccordion.append('<div class="title">' +
                                    '<i class="icon dropdown"></i>' + disciplina +
                                '</div>' +
                                '<div class="content">' +
                                    '<div class="field">' +
                                        '<select required id="ddlDificuldade' + discs[i] + '" name="ddlDificuldade' + discs[i] + '" class="ui search dropdown">' +
                                            '<option value="">Índice de Dificuldade</option>' + dificuldades +
                                        '</select>' +
                                    '</div>' +
                                '</div>');
            qteAccordion.append('<div class="title"><i class="icon dropdown"></i>' + disciplina + '</div>' +
                                '<div class="content">' +
                                    '<div class="two fields">' +
                                        '<div class="field objetiva"><label>Objetivas</label>' +
                                            '<input data-padrao="10" required id="txtQteObjetiva' + discs[i] + '" name="txtQteObjetiva' + discs[i] + '" data-mask="0#" type="number" placeholder="Quantidade de objetivas" min="1" />' +
                                        '</div>' +
                                        '<div class="field discursiva"><label>Discursivas</label>' +
                                            '<input data-padrao="10" required id="txtQteDiscursiva' + discs[i] + '" name="txtQteDiscursiva' + discs[i] + '" data-mask="0#" type="number" placeholder="Quantidade de discursivas" min="1" />' +
                                        '</div>' +
                                    '</div>' +
                                '</div>');
        };
        mostrarOpcoesPorTipo();
    }

    function mostrarOpcoesPorTipo() {
        var tipo = $('#ddlTipo').val();
        switch (tipo) {
            case '1':
                $('.discursiva').addClass('disabled').attr('style', 'pointer-events: none');
                $('.objetiva').removeClass('disabled').removeAttr('style');
                break;
            case '2':
                $('.discursiva').removeClass('disabled').removeAttr('style');
                $('.objetiva').addClass('disabled').attr('style', 'pointer-events: none');
                break;
            case '3':
                $('.discursiva').removeClass('disabled').removeAttr('style');
                $('.objetiva').removeClass('disabled').removeAttr('style');
                break;
            default:
                $('.discursiva').addClass('disabled').attr('style', 'pointer-events: none');
                $('.objetiva').addClass('disabled').attr('style', 'pointer-events: none');
        }
    }

    function usarValoresPadrao() {
        var $listaElementosValorPadrao = $('[data-padrao]');

        for (var i = 0, length = $listaElementosValorPadrao.length; i < length; i++) {
            var $elemento = $listaElementosValorPadrao.eq(i);
            var valorPadrao = $elemento.data('padrao');

            !$elemento.val() ? $elemento.val(valorPadrao) : null;

            if ($elemento.is('select'))
                $elemento.dropdown('set selected', $elemento.val());
        }
        prosseguir();
    }

    function confirmar() {
        var $modal = $('.ui.confirmar.modal');
        var $ddlDisciplinas = $('#ddlDisciplinas :selected');
        var $ddlTipo = $('#ddlTipo');
        var $table = $modal.find('tbody').html('');
        for (var i = 0; i < $ddlDisciplinas.length; i++) {
            $tr = $('<tr></tr>');
            $tdDisciplina = $('<td></td>').html('<b>' + $ddlDisciplinas.eq(i).text() + '</b>');
            $tdTemas = $('<td class="ui labels"></td>');
            $ddlTemas = $('#ddlTemas' + $ddlDisciplinas.eq(i).val() + ' :selected');
            for (var j = 0; j < $ddlTemas.length; j++) {
                $tdTemas.append($('<div class="ui tag label"></div>').text($ddlTemas.eq(j).text()));
            }
            $tdQteQuestoes = $('<td class="ui labels"></td>');
            if ($ddlTipo.val() == 1 || $ddlTipo.val() == 3) {
                $tdQteQuestoes.append($('<div class="ui label"></div>').html('Objetiva<div class="detail">' + $('#txtQteObjetiva' + $ddlDisciplinas.eq(i).val()).val() + '</div>'));
            }
            if ($ddlTipo.val() == 2 || $ddlTipo.val() == 3) {
                $tdQteQuestoes.append($('<div class="ui label"></div>').html('Discursiva<div class="detail">' + $('#txtQteDiscursiva' + $ddlDisciplinas.eq(i).val()).val() + '</div>'));
            }
            $tdDificuldade = $('<td></td>').text($('#ddlDificuldade' + $ddlDisciplinas.eq(i).val() + ' :selected').text());
            $table.append($tr.append($tdDisciplina).append($tdTemas).append($tdQteQuestoes).append($tdDificuldade));
        }
        var $checkBox = $('#chkAvalicoesSeparadas').parents('.ui.checkbox').clone();
        $checkBox.find('input').removeAttr('id name').attr('readonly', 'readonly');
        $checkBox.find('label').removeAttr('for');
        $modal.find('.ui.segment').html($checkBox);
        $modal.modal('show');
    }

    return {
        iniciar: iniciar
    }
})();